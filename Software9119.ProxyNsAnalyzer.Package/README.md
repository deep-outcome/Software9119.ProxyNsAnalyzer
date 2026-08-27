# Software9119.ProxyNsAnalyzer

`ProxyNsAnalyzer` is a C# code analyzer shipped with one rule and purpose only. It allows in conjunction with `.editorconfig` to define proxy
namespace that is then expected by analyzer in `.cs` files in scope of `.editorconfig` in question.

The rule is `PNANSDIFF`, _"Namespace does not match proxy namespace"_ and comes with default _warning_ priority.

Proxy namespace is declared in `.editorconfig` file using `proxy_namespace` key, e.g.

```editorconfig
[*.cs]

# namespace override
proxy_namespace = MySolution.MyFeature

# IDE0130: Namespace does not match folder structure
dotnet_diagnostic.IDE0130.severity = none
```

For more information, see [package repository Software9119.ProxyNsAnalyzer](https://github.com/deep-outcome/Software9119.ProxyNsAnalyzer).
