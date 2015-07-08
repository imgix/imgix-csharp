# Imgix-CSharp
Imgix Url Builder Library Ported to C#. [ImgIX](http://http://www.imgix.com/) is an image manipulation framework over HTTP with a variety of benefits.

# Installation
The library can be installed via NuGet: 

    Install-Package Imgix-CSharp

Or you can clone this repository and build it.

# Usage
The library can be invoked by newing up UrlBuilder with your imgix domain. You can optionally pass in a parameter for whether or not you want to use https.

    var builder = new UrlBuilder("domain.imgix.net", useHttps: true);
    
The constructor also accepts an array of domains, to support sharding:

    var builder = new UrlBuilder(new [] { "domain.imgix.net", "domain2.imgix.net" }, useHttps: true);

The UrlBuilder uses a dictionary (of key/value strings) called "Parameters" to specify the values you want to pass to imgIX along the queryString.
    builder.Parameters.Add("w", "400");
    builder.Parameters.Add("h", "300");
    
The UrlBuilder type also offers up a series of Properties:

### SignKey
This is the private key for signing requests, as specified in your imgIX source.

### ShardStrategy
This is the type of sharding you want to use, if you are supporting multiple imgIX domains. Options are CRC, Cycle, or None. If a SignKey is provided, the default is CRC. Cycle will round-robin through the available domains. CRC will build a Crc32 hash of the specified path, and mod it by the number of domains.

### SignWithLibrary
This is a parameter that allows you further sign your requests with the current version of the Imgix-CSharp library.

Finally, to construct your url, call BuildUrl() on your builder object, with the image path as your sole parameter:

    builder.BuildUrl("/users/1.png") // http://domain.imgix.net/users/1.png

[![Build Status](https://travis-ci.org/raynjamin/Imgix-CSharp.svg?branch=master)](https://travis-ci.org/raynjamin/Imgix-CSharp)
