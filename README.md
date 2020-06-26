<!-- ix-docs-ignore -->
![imgix logo](https://assets.imgix.net/sdk-imgix-logo.svg)

`imgix-csharp` is a client library for generating image URLs with [imgix](https://www.imgix.com/). 

[![Version](https://img.shields.io/nuget/v/imgix)](https://www.nuget.org/packages/Imgix/)
[![Build Status](https://travis-ci.org/imgix/Imgix-CSharp.svg?branch=main)](https://travis-ci.org/imgix/Imgix-CSharp)
![Downloads](https://img.shields.io/nuget/dt/imgix-csharp)
[![License](https://img.shields.io/github/license/imgix/imgix-csharp)](https://github.com/imgix/imgix-csharp/blob/main/LICENSE)

---
<!-- /ix-docs-ignore -->

- [Installation](#installation)
  - [Version 1.x](#version-1x)
- [Usage](#usage)
- [Signed URLs](#signed-urls)
- [Srcset Generation](#srcset-generation)
  - [Fixed-Width Images](#fixed-width-images)
      - [Variable Quality](#variable-quality)
  - [Fluid-Width Images](#fluid-width-images)
    - [Custom Widths](#custom-widths)
    - [Width Ranges](#width-ranges)
    - [Width Tolerance](#width-tolerance)
- [What is the `ixlib` param on every request?](#what-is-the-ixlib-param-on-every-request)
- [Code of Conduct](#code-of-conduct)

## Installation
The latest version of the library can be installed via NuGet:

    Install-Package Imgix

Or you can clone this repository and build it.

### Version 1.x

v1.x of the library can be installed via NuGet:

    Install-Package Imgix-CSharp


## Usage

You can start creating imgix URLs programmatically through `UrlBuilder` instances. The URL builder can be reused to create URLs for any images on the domains it is provided.

```csharp
using Imgix;
...
var builder = new UrlBuilder("domain.imgix.net");
var parameters = new Dictionary<String, String>();
parameters["w"] = "400";
parameters["h"] = "300";
Debug.Print(builder.BuildUrl("test.jpg", parameters));

// Prints out:
// https://domain.imgix.net/test.jpg?w=400&h=300
```

## Signed URLs

To produce a signed URL, you must enable secure URLs on your source and then provide your signature key to the URL builder.

```csharp
using Imgix;
...
var builder = new UrlBuilder("domain.imgix.net")
{
    SignKey = "aaAAbbBB11223344",
    IncludeLibraryParam = false
};
var parameters = new Dictionary<String, String>();
parameters["w"] = "500";
parameters["h"] = "1000";
Debug.Print(builder.BuildUrl("gaiman.jpg", parameters));

// Prints out:
// https://domain.imgix.net/gaiman.jpg?w=500&h=1000&s=fc4afbc39b6741560717142aeada876c
```

## Srcset Generation

The imgix-csharp library allows for generation of custom `srcset` attributes, which can be invoked through `BuildSrcSet()`. By default, the `srcset` generated will allow for responsive size switching by building a list of image-width mappings.

```csharp
var builder = new UrlBuilder("domain.imgix.net", "my-token", false, true);
String srcset = builder.BuildSrcSet("bridge.png");
Debug.Print(srcset);
```

Will produce the following attribute value, which can then be served to the client:

```html
https://domain.imgix.net/bridge.png?w=100&s=494158d968e94ac8e83772ada9a83ad1 100w,
https://domain.imgix.net/bridge.png?w=116&s=6a22236e189b6a9548b531330647ffa7 116w,
https://domain.imgix.net/bridge.png?w=135&s=cbf91f556dd67c0b9e26cb9784a83794 135w,
                                            ...
https://domain.imgix.net/bridge.png?w=7401&s=503e3ba04588f1c301863c9a5d84fe91 7401w,
https://domain.imgix.net/bridge.png?w=8192&s=152551ce4ec155f7a03f60f762a1ca33 8192w
```

### Fixed-Width Images

In cases where enough information is provided about an image's dimensions, `BuildSrcSet()` will instead build a `srcset` that will allow for an image to be served at different resolutions. The parameters taken into consideration when determining if an image is fixed-width are `w` (width), `h` (height), and `ar` (aspect ratio).

By invoking `BuildSrcSet()` with either a width **or** the height and aspect ratio (along with `fit=crop`, typically) provided, a different `srcset` will be generated for a fixed-size image instead.

```csharp
var builder = new UrlBuilder("domain.imgix.net", "my-token", false, true);
var parameters = new Dictionary<String, String>();
parameters["h"] = "200";
parameters["ar"] = "3:2";
parameters["fit"] = "crop";
String srcset = builder.BuildSrcSet("bridge.png", parameters);
Console.WriteLine(srcset);
```

Will produce the following attribute value:

```html
https://domain.imgix.net/bridge.png?h=200&ar=3%3A2&fit=crop&dpr=1&s=f39a78a6a2f245a70ba6aac910088435 1x,
https://domain.imgix.net/bridge.png?h=200&ar=3%3A2&fit=crop&dpr=2&s=d5dfd75bd777283d82975ab18a3091ff 2x,
https://domain.imgix.net/bridge.png?h=200&ar=3%3A2&fit=crop&dpr=3&s=8f25811130e3573530754c52f86a851d 3x,
https://domain.imgix.net/bridge.png?h=200&ar=3%3A2&fit=crop&dpr=4&s=ec348479a843a688c2ef9be487ea9be8 4x,
https://domain.imgix.net/bridge.png?h=200&ar=3%3A2&fit=crop&dpr=5&s=ce70bbfd682e683497f1afa6118ae2e3 5x
```

For more information to better understand `srcset`, we highly recommend [Eric Portis' "Srcset and sizes" article](https://ericportis.com/posts/2014/srcset-sizes/) which goes into depth about the subject.

##### Variable Quality

This library will automatically append a variable `q` parameter mapped to each `dpr` parameter when generating a [fixed-width image](#fixed-width-images) srcset. This technique is commonly used to compensate for the increased file size of high-DPR images.

Since high-DPR images are displayed at a higher pixel density on devices, image quality can be lowered to reduce overall file size without sacrificing perceived visual quality. For more information and examples of this technique in action, see [this blog post](https://blog.imgix.com/2016/03/30/dpr-quality).

This behavior will respect any overriding `q` value passed in as a parameter. Additionally, it can be disabled altogether by passing `disableVariableQuality = true` to `BuildSrcSet`.

This behavior specifically occurs when a [fixed-width image](#fixed-width-images) is rendered, for example:

```c#
UrlBuilder ub = new UrlBuilder(
    "demo.imgix.net",
    signKey: null,
    includeLibraryParam: false,
    useHttps: true);

var params = new Dictionary<string, string>() { { "w", "100" } };

String srcset = ub.BuildSrcSet("image.jpg", params); // Variable quality enabled by default.
```

The above will generate a srcset with the following `q` to `dpr` query `params`:

```html
https://demo.imgix.net/image.jpg?dpr=1&q=75&w=100 1x,
https://demo.imgix.net/image.jpg?dpr=2&q=50&w=100 2x,
https://demo.imgix.net/image.jpg?dpr=3&q=35&w=100 3x,
https://demo.imgix.net/image.jpg?dpr=4&q=23&w=100 4x,
https://demo.imgix.net/image.jpg?dpr=5&q=20&w=100 5x
```

### Fluid-Width Images

#### Custom Widths
In situations where specific widths are desired when generating `srcset` pairs, a user can specify them by passing an array of positive integers as `widths`:

```c#
UrlBuilder ub = new UrlBuilder(
    "demo.imgix.net",
    signKey: null,
    includeLibraryParam: false,
    useHttps: true);

int[] widths = { 144, 240, 320, 446, 640 };
Dictionary<string, string> parameters = new Dictionary<string, string>();
String srcset = ub.BuildSrcSet("image.jpg", parameters, widths.ToList());
```

```html
https://demo.imgix.net/image.jpg?w=144 144w,
https://demo.imgix.net/image.jpg?w=240 240w,
https://demo.imgix.net/image.jpg?w=320 320w,
https://demo.imgix.net/image.jpg?w=446 446w,
https://demo.imgix.net/image.jpg?w=640 640w
```

**Note**: in situations where a `srcset` is being rendered as a [fixed image](#fixed-image-rendering), any custom `widths` passed in will be ignored.

Additionally, if both `widths` and a width `tol`erance are passed to the `BuildSrcSet` method, the custom widths list will take precedence.

#### Width Ranges

In certain circumstances, you may want to limit the minimum or maximum value of the non-fixed (fluid-width) `srcset` generated by the `BuildSrcSet` method. To do this, you can specify the widths at which a srcset should `begin` and `end`:

```c#
UrlBuilder ub = new UrlBuilder(
    "demo.imgix.net",
    signKey: null,
    includeLibraryParam: false,
    useHttps: true);

Dictionary<string, string> parameters = new Dictionary<string, string>();
String srcset = ub.BuildSrcSet("image.jpg", parameters, 500, 2000);
```

Formatted version of the above srcset attribute:

``` html
https://demo.imgix.net/image.jpg?w=500 500w,
https://demo.imgix.net/image.jpg?w=580 580w,
https://demo.imgix.net/image.jpg?w=673 673w,
https://demo.imgix.net/image.jpg?w=780 780w,
https://demo.imgix.net/image.jpg?w=905 905w,
https://demo.imgix.net/image.jpg?w=1050 1050w,
https://demo.imgix.net/image.jpg?w=1218 1218w,
https://demo.imgix.net/image.jpg?w=1413 1413w,
https://demo.imgix.net/image.jpg?w=1639 1639w,
https://demo.imgix.net/image.jpg?w=1901 1901w,
https://demo.imgix.net/image.jpg?w=2000 2000w
```

#### Width Tolerance

The `srcset` width `tol`erance dictates the maximum `tol`erated difference between an image's downloaded size and its rendered size.

For example, setting this value to 0.1 means that an image will not render more than 10% larger or smaller than its native size. In practice, the image URLs generated for a width-based srcset attribute will grow by twice this rate.

A lower tolerance means images will render closer to their native size (thereby increasing perceived image quality), but a large srcset list will be generated and consequently users may experience lower rates of cache-hit for pre-rendered images on your site.

By default, srcset width `tol`erance is set to 0.08 (8 percent), which we consider to be the ideal rate for maximizing cache hits without sacrificing visual quality. Users can specify their own width tolerance by providing a positive scalar value as width `tol`erance:

```csharp
UrlBuilder ub = new UrlBuilder(
    "demo.imgix.net",
    signKey: null,
    includeLibraryParam: false,
    useHttps: true);

Dictionary<string, string> parameters = new Dictionary<string, string>();
String srcset = ub.BuildSrcSet("image.jpg", parameters, 100, 384, 0.20);
```

In this case, the width `tol`erance is set to 20 percent, which will be reflected in the difference between subsequent widths in a srcset pair:

```html
https://demo.imgix.net/image.jpg?w=100 100w,
https://demo.imgix.net/image.jpg?w=140 140w,
https://demo.imgix.net/image.jpg?w=196 196w,
https://demo.imgix.net/image.jpg?w=274 274w,
https://demo.imgix.net/image.jpg?w=384 384w
```

## What is the `ixlib` param on every request?

For security and diagnostic purposes, we sign all requests with the language and version of library used to generate the URL.

This can be disabled by passing `false` for the `includeLibraryParam` option to `new UrlBuilder`:

```csharp
using Imgix;
...
var builder = new UrlBuilder("domain.imgix.net", includeLibraryParam: false);
```

## Code of Conduct
Users contributing to or participating in the development of this project are subject to the terms of imgix's [Code of Conduct](https://github.com/imgix/code-of-conduct).
