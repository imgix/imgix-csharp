using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgix_CSharp
{
    public class URLHelper
    {
        public String domain { get; set; }
        public String path { get; set; }
        public String scheme { get; set; }
        public String SignKey { get; set; }

        public Dictionary<String, String> Parameters { get; set; }

        public URLHelper(String domain, String path, String scheme, String signKey, Dictionary<String, String> parameters) 
        {
            this.domain = domain;
		    if (!path.StartsWith("/")) {
			    path = "/" + path;
		    }
		    this.path = path;
		    this.scheme = scheme;
		    SignKey = signKey;
		    Parameters = new Dictionary<String, String>(parameters);
	    }

        public URLHelper(String domain, String path, String scheme, String signKey)
            : this(domain, path, scheme, signKey, new Dictionary<String, String>())
        {
            
        }

	    public URLHelper(String domain, String path, String scheme) : this(domain, path, scheme, "")
        {
	    }

        public URLHelper(String domain, String path) : this(domain, path, "http")
        {
	    }

	    public void setParameter(String key, String value)
	    {
	        Parameters[key] = value;
	    }

	    public void setParameter(String key, Int32 value)
	    {
	        Parameters[key] = value.ToString();
	    }

	    public void deleteParameter(String key)
	    {
	        Parameters.Remove(key);
	    }

	    public String getURL() {
		    List<String> queryPairs = new LinkedList<String>();

		    for (Entry<String, String> entry : parameters.entrySet()) {
			    String k = entry.getKey();
			    String v = entry.getValue();
			    queryPairs.add(k + "=" + encodeURIComponent(v));
		    }

		    String query = joinList(queryPairs, "&");

		    if (signKey != null && signKey.length() > 0) {
			    String newPath = path;
			    String restPath = path.substring(1);
			    if (URLHelper.decodeURIComponent(restPath).equals(restPath)) {
				    newPath = "/" + encodeURIComponent(newPath.substring(1));
			    }
			    String delim = query.equals("") ? "" : "?";
			    String toSign = signKey + newPath + delim + query;
			    String signature = MD5(toSign);

			    if (query.length() > 0) {
				    query += "&s=" + signature;
			    } else {
				    query = "s=" + signature;
			    }

			    return buildURL(scheme, domain, newPath, query);
		    }

		    return buildURL(scheme, domain, path, query);
	    }

	public String toString() {
		return getURL();
	}

	///////////// Static

	private static String buildURL(String scheme, String host, String path, String query) {
		// do not use URI to build URL since it will do auto-encoding which can break our previous signing
		String url = String.format("%s://%s%s?%s", scheme, host, path, query);
		if (url.endsWith("#")) {
			url = url.substring(0, url.length() - 1);
		}

		if (url.endsWith("?")) {
			url = url.substring(0, url.length() - 1);
		}

		return url;
	}

	private static String MD5(String md5) {
	   try {
			MessageDigest md = MessageDigest.getInstance("MD5");
			byte[] array = md.digest(md5.getBytes("UTF-8"));
			StringBuffer sb = new StringBuffer();
			for (int i = 0; i < array.length; ++i) {
			  sb.append(Integer.toHexString((array[i] & 0xFF) | 0x100).substring(1,3));
		   }
			return sb.toString();
		} catch (UnsupportedEncodingException e) {
		} catch (NoSuchAlgorithmException e) {
		}
		return null;
	}

	private static String joinList(List<String> strings, String separator) {
		StringBuilder sb = new StringBuilder();
		String sep = "";
		for(String s: strings) {
			sb.append(sep).append(s);
			sep = separator;
		}
		return sb.toString();
	}

	public static String encodeURIComponent(String s) {
		String result = null;

		try {
		  result = URLEncoder.encode(s, "UTF-8")
							 .replaceAll("\\+", "%20")
							 .replaceAll("\\%21", "!")
							 .replaceAll("\\%27", "'")
							 .replaceAll("\\%28", "(")
							 .replaceAll("\\%29", ")")
							 .replaceAll("\\%7E", "~");
		} catch (UnsupportedEncodingException e) {
		  result = s;
		}

		return result;
	}

	public static String decodeURIComponent(String s) {
		if (s == null) {
			return null;
		}

		String result = null;

		try {
			result = URLDecoder.decode(s, "UTF-8");
		} catch (UnsupportedEncodingException e) {
			result = s;
		}

		return result;
  }


    }
}
