# CodeCapital.AspNetCore

ASP.NET Core libraries.

[![.NET](https://github.com/codecapital/CodeCapital.AspNetCore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/codecapital/CodeCapital.AspNetCore/actions/workflows/dotnet.yml)

CodeCapital.AspNetCore.Mvc.TagHelpers

[![NuGet](https://img.shields.io/nuget/v/CodeCapital.AspNetCore.Mvc.TagHelpers.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CodeCapital.AspNetCore.Mvc.TagHelpers)

- Json Flattener
- Tag Helpers
  - asp-class
  - asp-if \[negate\]
- Razor to String Renderer
- Configuration in ASP.NET Core
  - AddRemoteJsonFile(url)
- Services
  - MailGun
  - ReCaptcha v2
- RazorLibrary
  - Calendar (blazor)
  - Table (blazor, with column sorting)


## Json Flattener
*Namespace: CodeCapital.System.Text.Json*

This library flattens json string into
```List<dynamic>```.

```c#
var flattener = new JsonFlattener();
var list = flattener.Flatten(json);
```


## ASP.NET Core TagHelpers
*Namespace: CodeCapital.AspNetCore.Mvc.TagHelpers*

### Conditional Add Class (asp-class-something="")

ASP.NET Core
```aspnet
    <p asp-class-text-success="@Model.IsSuccess">Hello, World!</p>
```
HTML (If a condition is true, a class attribute is added to html tag)
```html
    <p class="text-success">Hello, World!</p>
```

### If Condition (asp-if="", &lt;asp-if&gt;&lt;/asp-if&gt; )

ASP.NET Core
```aspnet
<asp-if condition="@Model.IsSuccess">
    <p>Hello, World!</p>
<asp-if>

<p asp-if="@Model.IsSucces">Hello, World!</p>
```
HTML (If a condition is true)
```html
    <p>Hello, World!</p>
```
#### Negate
ASP.NET Core
```aspnet
<asp-if condition="@Model.IsSuccess" negate>
    <p>Hello, World!</p>
<asp-if>

<p asp-if="@Model.IsSucces" negate>Hello, World!</p>
```
HTML (If a condition is true, nothing renders.)
```html
```

## References
https://stackoverflow.com/questions/58512542/read-a-json-file-and-generate-string-keys-with-values-in-a-dictionary-like-objec
https://stackoverflow.com/questions/7394551/c-sharp-flattening-json-structure

