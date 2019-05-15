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

## Domain Sharded URLs
**Warning: Domain Sharding has been deprecated and will be removed in the next major release**<br>
To find out more, see our [blog post](https://blog.imgix.com/2019/05/03/deprecating-domain-sharding) explaining the decision to remove this feature.

Domain sharding enables you to spread image requests across multiple domains. This allows you to bypass the requests-per-host limits of browsers. We recommend 2-3 domain shards maximum if you are going to use domain sharding.

In order to use domain sharding, you need to add multiple domains to your source. You then provide an array of these domains to a builder.

```csharp
using Imgix;
...
var domains = new [] { "demos-1.imgix.net", "demos-2.imgix.net", "demos-3.imgix.net" };
var builder = new UrlBuilder(domains);
var parameters = new Dictionary<String, String>();
parameters["w"] = "100";
parameters["h"] = "100";
Debug.Print(builder.BuildUrl("bridge.png", parameters));
Debug.Print(builder.BuildUrl("flower.png", parameters));

// Prints out:
// http://demos-1.imgix.net/bridge.png?h=100&w=100
// http://demos-2.imgix.net/flower.png?h=100&w=100
```

By default, shards are calculated using a checksum so that the image path always resolves to the same domain. This improves caching in the browser.

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
