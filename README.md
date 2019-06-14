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

## Code of Conduct
Users contributing to or participating in the development of this project are subject to the terms of imgix's [Code of Conduct](https://github.com/imgix/code-of-conduct).
