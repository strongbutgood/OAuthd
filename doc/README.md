OAuthd
======

Things to know:
------

1. JWS (JSON Web Signature) - see [RFC](https://datatracker.ietf.org/doc/html/rfc7515#section-5.2) for validation of signed content
2. sjcl - clues us in to how KJUR.crypto.MessageDigest receives parameters through [update](https://bitwiseshiftleft.github.io/sjcl/doc/sha256.js.html#line64) function signature
3. JWT (JSON Web Token) - see [RFC](https://datatracker.ietf.org/doc/html/draft-jones-json-web-token-10#section-3.1) for example content returned
4. Oidc (OpenId Connect) - see [documentation](https://identitymodel.readthedocs.io/en/latest/native/manual.html) for how it could be used?
5. Oidc (OpenId Connect) - see [wikipedia](https://en.wikipedia.org/wiki/OpenID#OpenID_Connect_(OIDC)) for background into maybe what is happening
6. mitmproxy (man-in-the-middle proxy) - see [documentation](https://docs.mitmproxy.org/stable/overview-getting-started/) for how to intercept comms with site
7. 

.well-known/openid-configuration
------

```
{
    "authorization_endpoint": "https://ac1vs03/ASTS/connect/authorize",
    "check_session_iframe": "https://ac1vs03/ASTS/connect/checksession",
    "end_session_endpoint": "https://ac1vs03/ASTS/connect/endsession",
    "grant_types_supported": [
        "authorization_code",
        "client_credentials",
        "password",
        "refresh_token",
        "implicit"
    ],
    "id_token_signing_alg_values_supported": [
        "RS256"
    ],
    "issuer": "https://ac1vs03/ASTS",
    "jwks_uri": "https://ac1vs03/ASTS/.well-known/jwks",
    "response_modes_supported": [
        "form_post",
        "query",
        "fragment"
    ],
    "response_types_supported": [
        "code",
        "token",
        "id_token",
        "id_token token",
        "code id_token",
        "code token",
        "code id_token token"
    ],
    "scopes_supported": [
        "openid",
        "profile",
        "roles",
        "system"
    ],
    "subject_types_supported": [
        "public"
    ],
    "token_endpoint": "https://ac1vs03/ASTS/connect/token",
    "userinfo_endpoint": "https://ac1vs03/ASTS/connect/userinfo"
}
```

Authorisation
------

### Initial Request

Using the well-known `authorization_endpoint`, the following needs to be provided in the GET url:

- `state=...` where ... is some randomised number
- `nonce=...` where ... is some randomised number
- `client_id=AC1VS03%5CRecipe%20Manager%20Plus` or uri encoded AC1VS03\Recipe Manager Plus
- `redirect_uri=https%3A%2F%2Fac1vs03%2FRecipeManagerPlus%2Fsts_callback.cshtml` or uri encoded https://ac1vs03/RecipeManagerPlus/sts_callback.cshtml
- `response_type=id_token%20token`
- `scope=openid%20profile%20system`

The uris can be encoded with `Uri.EscapeDataString(string)`.

The response will be a redirect to https://ac1vs03/ASTS/login?signin=... where ... is some hex string data
along with a couple of cookies to be set. Redirecting with cookies in hand correctly should land us at the login page.

### Login Page

All going well the login page content will contain a `script` html element with a model JSON string (CDATA has been added for readability):
```
  <script id='modelJson' type='application/json'><![CDATA[
    {
        "loginUrl":"/ASTS/login?signin=59f08bc95b32c8341f2373222f5cf3fe",
        "antiForgery":
        {
            "name":"idsrv.xsrf",
            "value":"fSL17h4XT9_lE59ianfk24DnI8GXiJmCp2Y7AjhkUv5XlxRz8m1ESWh3E4tcSz_M4Ou6KF_8-vHVv3-IcU_VKRnQeNE60JoEnps0__bHvbg"
        },
        "allowRememberMe":false,
        "rememberMe":false,
        "username":"administrator",
        "externalProviders":[],
        "additionalLinks":null,
        "errorMessage":null,
        "requestId":"528e6d1d-0413-4af5-b9d9-f0284f87e83c",
        "siteUrl":"https://ac1vs03/ASTS/",
        "siteName":"User Login",
        "currentUser":null,
        "logoutUrl":"https://ac1vs03/ASTS/logout"
    }
  ]]></script>
```

Additionally the login page html content uses angular, and in the following content where you see `model.xyz` it is referring to the modelJson script content:
```
              <form class="form" name="form" method="post" action="{{model.loginUrl}}">
                <anti-forgery-token token="model.antiForgery"></anti-forgery-token>
                <fieldset>
                  <div class="input">
                    <label for="username">User name</label>
                    <input required name="username" autofocus id="username" type="text" class="form-control" placeholder="User name" ng-model="model.username">
                  </div>
                  <div class="input">
                    <label for="password">Password</label>
                    <input required id="password" name="password" type="password" class="form-control" placeholder="Password" ng-model="model.password">
                  </div>
                  <br />
                  <br />
                  <div class="input alignRight">
                    <button id="submitLogin" class="wwButton hot">Login</button>
                  </div>
                </fieldset>
              </form>
```

So we POST to the `{{model.loginUrl}}` using the form content as `application/x-www-form-urlencoded`.
In the above case that would look like (with the password omitted for obvious reasons):
```
idsrv.xsrf: fSL17h4XT9_lE59ianfk24DnI8GXiJmCp2Y7AjhkUv5XlxRz8m1ESWh3E4tcSz_M4Ou6KF_8-vHVv3-IcU_VKRnQeNE60JoEnps0__bHvbg
username:   administrator
password:   *********
```

The response from this is a redirect to https://ac1vs03/ASTS/connect/authorize?... with more cookies to be set,
Again redirecting with cookies in hand, this time we get another redirect to https://ac1vs03/RecipeManagerPlus/sts_callback.cshtml#... which has
url encoded information:

- `id_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlREeFN4dFZQNlYtbFNJV1UyYlFKV0ZuNEMxQSIsImtpZCI6IlREeFN4dFZQNlYtbFNJV1UyYlFKV0ZuNEMxQSJ9.eyJub25jZSI6IjE1MDk1ODI5NTE1MDg4NjM1IiwiaWF0IjoxNjMwMjk1NzA5LCJhdF9oYXNoIjoicUR2Q1Y1ckpPOExheC1jdmZ4RVJmQSIsInN1YiI6IkFDMVZTMDNcXEFETUlOSVNUUkFUT1IiLCJhbXIiOiJwYXNzd29yZCIsImF1dGhfdGltZSI6IjE2MzAyOTU3MDUiLCJpZHAiOiJpZHNydiIsImlzcyI6Imh0dHBzOi8vYWMxdnMwMy9BU1RTIiwiYXVkIjoiQUMxVlMwM1xcUmVjaXBlIE1hbmFnZXIgUGx1cyIsImV4cCI6MTYzMDMzMTcwOSwibmJmIjoxNjMwMjk1NzA5fQ.CPR-8LmBZO-ZwX2HiaSEvh-JJwZEYzV3AvrcFn99pinbaFh1-J7ZcTsqC8k_8EUcI2YWSGUl0P7QAVSyPBIbxclWnwOcabG7iPGhmDgt2nFWuZ0vZMDO9XMkmFsDsuBl1rrvFna5yVzttTpj_eQHtlGL9qVDKekvmazhSXRk3jeNMGvSeXy8AyhivpHMaZGdn8zg57e24-EZreZOqh5zzs0M4tQsW7vJOYRpy8EybvUFANcz5HB1U9LScfUUXW5pUEJpmtatYzXfKmfT8BA2F7-huSo2fxewwbNo5hHwEm2aZSIuYdDdstNM5Zz3CwQihUk45RWTRvrX56tJZneSVQ` a JWT as above
- `access_token=9517c2bac1da664bd7658d72fd38f43e` to be used in future api calls
- `token_type=Bearer` indicating that the `access_token` needs to be set as authorisation header in future api calls_
- `expires_in=36000` number of seconds until `access_token` expires
- `scope=openid%20profile%20system`
- `state=5132675135849685`
- `session_state=A_WOksP5pRp_62Wcjn9YslOmI5_QS9rAsU3o1FbZEbA.e61f545b5f600582f7eb0c845f51ec39`

This url encoded information needs to be vaidated with the `id_token` head.payload.signature,
the `access_token` stored for later, and following the redirect finally gives us a logging in page in which the javascript calls `tokenManager.processTokenCallbackAsync()`
which on completion (or failure) executes:
```
window.location = "/recipemanagerplus/index.cshtml" + hash
```

Now the application is/should be up and running.
