Ixoxo.BundleCompiler
====================

This a `.bundle` file compiler.

Compiles Web Essentials `.bundle` files in command line, useful as build step for continuous integration. This also means that only bundle files and source files need to commited to source control. 

**Command Prompt / Powershell example.**

```dos 
BundleCompiler.exe js/bundle.js.bundle
```
The above code would generate `bundle.js`, `bundle.min.js` and `bundle.min.js.map` in the same directory as `bundle.js.bundle`
