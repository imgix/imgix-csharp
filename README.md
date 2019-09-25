# Imgix-CSharp

[![Build Status](https://travis-ci.org/imgix/Imgix-CSharp.svg?branch=master)](https://travis-ci.org/imgix/Imgix-CSharp)

imgix URL builder library written in C#. [imgix](http://www.imgix.com/) is an image manipulation framework over HTTP with a variety of benefits.

## Installation
The latest version of the library can be installed via NuGet:

    Install-Package Imgix

Or you can clone this repository and build it.

### Version 1.x

v1.x of the library can be installed via NuGet:

    Install-Package Imgix-CSharp


## Basic Usage

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

## What is the `ixlib` param on every request?

For security and diagnostic purposes, we sign all requests with the language and version of library used to generate the URL.

This can be disabled by passing `false` for the `includeLibraryParam` option to `new UrlBuilder`:

```csharp
using Imgix;
...
var builder = new UrlBuilder("domain.imgix.net", includeLibraryParam: false);
```

Srcset Generation
-----------

The imgix-csharp library allows for generation of custom `srcset` attributes, which can be invoked through `BuildSrcSet()`. By default, the `srcset` generated will allow for responsive size switching by building a list of image-width mappings.

```csharp
var builder = new UrlBuilder("domain.imgix.net", "my-token", false, true);
String srcset = ub.BuildSrcSet("bridge.png");
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
String srcset = ub.BuildSrcSet("bridge.png", parameters);
Debug.Print(srcset);
```

Will produce the following attribute value:

```html
https://domain.imgix.net/bridge.png?ar=3%3A2&dpr=1&fit=crop&h=200&s=4c79373f535df7e2594a8f6622ec6631 1x,
https://domain.imgix.net/bridge.png?ar=3%3A2&dpr=2&fit=crop&h=200&s=dc818ae4522494f2f750651304a4d825 2x,
https://domain.imgix.net/bridge.png?ar=3%3A2&dpr=3&fit=crop&h=200&s=ba1ec0cef6c77ff02330d40cc4dae932 3x,
https://domain.imgix.net/bridge.png?ar=3%3A2&dpr=4&fit=crop&h=200&s=b51e497d9461be62354c0ea12b6524fb 4x,
https://domain.imgix.net/bridge.png?ar=3%3A2&dpr=5&fit=crop&h=200&s=dc37c1fbee505d425ca8e3764b37f791 5x
```

For more information to better understand `srcset`, we highly recommend [Eric Portis' "Srcset and sizes" article](https://ericportis.com/posts/2014/srcset-sizes/) which goes into depth about the subject.

## Code of Conduct
Users contributing to or participating in the development of this project are subject to the terms of imgix's [Code of Conduct](https://github.com/imgix/code-of-conduct).
