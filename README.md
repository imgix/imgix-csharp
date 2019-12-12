<!-- ix-docs-ignore -->
![imgix logo](https://assets.imgix.net/sdk-imgix-logo.svg)

`imgix-csharp` is a client library for generating image URLs with [imgix](https://www.imgix.com/). 

[![Version](https://img.shields.io/nuget/v/imgix)](https://www.nuget.org/packages/Imgix/)
[![Build Status](https://travis-ci.org/imgix/Imgix-CSharp.svg?branch=master)](https://travis-ci.org/imgix/Imgix-CSharp)
![Downloads](https://img.shields.io/nuget/dt/imgix-csharp)
[![License](https://img.shields.io/github/license/imgix/imgix-csharp)](https://github.com/imgix/imgix-csharp/blob/master/LICENSE)

---
<!-- /ix-docs-ignore -->

- [Installation](#installation)
- [Usage](#Usage)
- [Signed URLs](#signed-urls)
- [Srcset Generation](#srcset-generation)
- [What is the ixlib param on every request?](#what-is-the-ixlib-param-on-every-request)
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
https://domain.imgix.net/bridge.png?w=134&s=cbf91f556dd67c0b9e26cb9784a83794 134w,
                                            ...
https://domain.imgix.net/bridge.png?w=7400&s=503e3ba04588f1c301863c9a5d84fe91 7400w,
https://domain.imgix.net/bridge.png?w=8192&s=152551ce4ec155f7a03f60f762a1ca33 8192w
```

In cases where enough information is provided about an image's dimensions, `BuildSrcSet()` will instead build a `srcset` that will allow for an image to be served at different resolutions. The parameters taken into consideration when determining if an image is fixed-width are `w` (width), `h` (height), and `ar` (aspect ratio). By invoking `BuildSrcSet()` with either a width **or** the height and aspect ratio (along with `fit=crop`, typically) provided, a different `srcset` will be generated for a fixed-size image instead.

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
