using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace _0Zero.SandBox {
  public class SandBoxer : IDisposable {
    /// <summary>
    /// The currently sandboxed object.
    /// </summary>
    public SandBoxedObj SandBox { get; private set; }
    private AppDomain _sandboxAD { get; set; }
    private string _tempPath { get; set; }

    /// <summary>
    /// Create a new SandBoxer Instance (Untrusted)
    /// </summary>
    /// <param name="code">The C# code to be compiled.</param>
    /// <param name="AdditionalPermissions">A list of additional permissions allowed for this untrusted code.</param>
    public SandBoxer(string code, params IPermission[] AdditionalPermissions) {
      this.Init(code, true, AdditionalPermissions);
    }
    /// <summary>
    /// Create a new SandBoxer Instance
    /// </summary>
    /// <param name="code">The C# code to be compiled.</param>
    /// <param name="isTrustedCode">Flag to specify if code is to be executed with trusted permissions.</param>
    public SandBoxer(string code, bool isTrustedCode) {
      this.Init(code, !isTrustedCode);
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
    private void Init(string code, bool Untrusted = true, params IPermission[] AdditionalPermissions) {
      this._tempPath = Path.GetTempPath();
      if(!this._tempPath.EndsWith("\\")) this._tempPath += "\\";

      Guid tmpfolder = Guid.NewGuid();

      while(Directory.Exists(this._tempPath + tmpfolder.ToString())) {
        tmpfolder = Guid.NewGuid();
      }

      this._tempPath += tmpfolder.ToString();
      Directory.CreateDirectory(this._tempPath);
      try {
        if(Untrusted) {
          AppDomainSetup ads = new AppDomainSetup();
          PermissionSet pset = new PermissionSet(PermissionState.None);
          // we have to full trust the calling class (this)
          StrongName fullTrustAssembly = typeof(SandBoxer).Assembly.Evidence.GetHostEvidence<StrongName>();

          pset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
          pset.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, this._tempPath));

          if(AdditionalPermissions != null && AdditionalPermissions.Count() > 0) {
            foreach(IPermission perm in AdditionalPermissions) {
              pset.AddPermission(perm);
            }
          }

          ads.ApplicationName = "Sandbox " + tmpfolder.ToString();
          ads.ApplicationBase = this._tempPath;
          ads.DynamicBase = this._tempPath;
          ads.DisallowCodeDownload = true;
          ads.DisallowPublisherPolicy = true;
          ads.DisallowBindingRedirects = true;

          // Create the sandboxed domain.
          this._sandboxAD = AppDomain.CreateDomain(
            "Sandboxed Domain " + tmpfolder.ToString(),
            null,
            ads,
            pset,
            fullTrustAssembly);
          AssemblyName Dll = CompileCode(code, this._tempPath, tmpfolder);

          ObjectHandle handle = Activator.CreateInstanceFrom(this._sandboxAD, typeof(SandBoxedObj).Assembly.ManifestModule.FullyQualifiedName, typeof(SandBoxedObj).FullName);
          this.SandBox = (SandBoxedObj)handle.Unwrap();
          this.SandBox.SetInfo(Dll);
        } else {
          // fully trusted
          this.SandBox = new SandBoxedObj();
          this.SandBox.SetInfo(CompileCode(code, this._tempPath, Guid.NewGuid()));
        }
      } catch(Exception ex) {
        this.Dispose();
        throw ex;
      }
    }

    private static AssemblyName CompileCode(string code, string BasePath, Guid id) {
      string DllPath = "";

      PermissionSet pset = new PermissionSet(PermissionState.Unrestricted);
      AppDomainSetup ads = new AppDomainSetup();

      // we have to full trust the calling class (this)
      StrongName fullTrustAssembly = typeof(SandBoxer).Assembly.Evidence.GetHostEvidence<StrongName>();

      ads.ApplicationName = "CodeCompile " + id.ToString();
      ads.ApplicationBase = BasePath;
      ads.DynamicBase = BasePath;
      ads.DisallowCodeDownload = false;
      ads.DisallowPublisherPolicy = false;
      ads.DisallowBindingRedirects = false;

      // Create the sandboxed domain.
      AppDomain ccAD = AppDomain.CreateDomain(
        "CodeCompile Domain " + id.ToString(),
        null,
        ads,
        pset,
        fullTrustAssembly);
      ObjectHandle handle = Activator.CreateInstanceFrom(ccAD, typeof(CodeCompile).Assembly.ManifestModule.FullyQualifiedName, typeof(CodeCompile).FullName);
      CodeCompile cc = (CodeCompile)handle.Unwrap();
      DllPath = BasePath + "\\DynamicCodeGen." + id.ToString() + ".dll";
      AssemblyName an = cc.Compile(code, DllPath);
      AppDomain.Unload(ccAD);
      GC.Collect(); // collects all unused memory
      GC.WaitForPendingFinalizers(); // wait until GC has finished its work
      GC.Collect();
      return an;
    }

    private class CodeCompile : MarshalByRefObject {
      public CodeCompile() { }

      internal AssemblyName Compile(string code, string DllPath) {
        CSharpCodeProvider _cp = new CSharpCodeProvider();
        CompilerParameters _cparams = new CompilerParameters();

        _cparams.GenerateExecutable = false;
        _cparams.GenerateInMemory = false;
        _cparams.OutputAssembly = DllPath;
        _cparams.IncludeDebugInformation = false;

        _cparams.ReferencedAssemblies.Add(Assembly.GetCallingAssembly().Location);
        foreach(AssemblyName n in Assembly.GetCallingAssembly().GetReferencedAssemblies()) {
          Assembly a = Assembly.Load(n);
          if(a == null || a.IsDynamic) continue;
          _cparams.ReferencedAssemblies.Add(a.Location);
        }


        CompilerResults results = _cp.CompileAssemblyFromSource(_cparams, code);

        if(results.Errors.HasErrors) {
          StringBuilder sb = new StringBuilder();

          foreach(CompilerError error in results.Errors) {
            sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
          }

          throw new InvalidOperationException(sb.ToString());
        }

        if(results == null || results.CompiledAssembly == null) {
          throw new InvalidOperationException("Code did not generate an assembly!");
        }

        return results.CompiledAssembly.GetName();
      }
    }

    public void Dispose() {
      if(this._sandboxAD != null) {
        AppDomain.Unload(this._sandboxAD);
        GC.Collect(); // collects all unused memory
        GC.WaitForPendingFinalizers(); // wait until GC has finished its work
        GC.Collect();
      };
      var loadedAsms = AppDomain.CurrentDomain.GetAssemblies();

      if(!String.IsNullOrWhiteSpace(this._tempPath) && Directory.Exists(this._tempPath)) {
        try {
          Directory.Delete(this._tempPath, true);
        } catch { }
      }
    }

    public class SandBoxedObj : MarshalByRefObject {
      private Assembly _compiledAssembly = null;

      public SandBoxedObj() { }

      internal void SetInfo(AssemblyName Dll) {
        this._compiledAssembly = AppDomain.CurrentDomain.Load(Dll);
      }

      private MethodInfo getMethodInfo(string FullClassName, string MethodName, Type[] paramTypes) {
        Type p = this._compiledAssembly.GetType(FullClassName);
        if(p != null) {
          MethodInfo ret = p.GetMethod(MethodName, paramTypes);
          if(ret != null && ret.IsStatic) {
            return ret;
          } else {
            throw new InvalidOperationException(String.Format("Static Method does not exist in Class or Type from generated assembly! [{0}, {1}({2})]", FullClassName, MethodName, String.Join(", ", paramTypes.Select(t => t.FullName))));
          }
        } else {
          throw new InvalidOperationException(String.Format("Class or Type does not exist in generated assembly! [{0}]", FullClassName));
        }
      }

      private MethodInfo getMethodInfo(object Instance, string MethodName, Type[] paramTypes) {
        if(Instance == null) {
          throw new ArgumentNullException("Instance", "Instance cannot be null!");
        }
        Type p = Instance.GetType();
        if(p != null) {
          MethodInfo ret = p.GetMethod(MethodName, paramTypes);
          if(ret != null && !ret.IsStatic) {
            return ret;
          } else {
            throw new InvalidOperationException(String.Format("Instance Method does not exist in Class or Type from generated assembly! [{0}, {1}({2})]", Instance.GetType().FullName, MethodName, String.Join(", ", paramTypes.Select(t => t.FullName))));
          }
        } else {
          throw new InvalidOperationException(String.Format("Class or Type does not exist in generated assembly! [{0}]", Instance.GetType().FullName));
        }
      }

      /// <summary>
      /// Create an instance of an object from the compiled code.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="ContructorParameters">A list of parameters to be passed into the contructor for the object.</param>
      /// <returns>A new instance of the object.</returns>
      public object GetInstance(string FullClassName, params object[] ContructorParameters) {
        Type p = this._compiledAssembly.GetType(FullClassName);
        if(p != null) {
          object ret = Activator.CreateInstance(p, ContructorParameters);
          if(ret != null) {
            return ret;
          } else {
            throw new InvalidOperationException(String.Format("Unable to find matching constructor in Class or Type in generated assembly! [{0}({1})]", FullClassName, String.Join(", ", ContructorParameters.Select(t => t.GetType().FullName))));
          }
        } else {
          throw new InvalidOperationException(String.Format("Class or Type does not exist in generated assembly! [{0}]", FullClassName));
        }
      }

      /// <summary>
      /// Call a static method with 0 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call(string FullClassName, string MethodName) {
        try {
          this.Call(
              this.getMethodInfo(FullClassName, MethodName, new Type[0])
            );
        } catch(Exception) {


          throw;
        }
      }

      /// <summary>
      /// Call a static method with 1 parameter, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1>(string FullClassName, string MethodName, T1 arg1) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1)
          }),
          arg1
        );
      }

      /// <summary>
      /// Call a static method with 2 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2>(string FullClassName, string MethodName, T1 arg1, T2 arg2) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2)
          }),
          arg1, arg2
        );
      }

      /// <summary>
      /// Call a static method with 3 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3)
          }),
          arg1, arg2, arg3
        );
      }

      /// <summary>
      /// Call a static method with 4 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4)
          }),
          arg1, arg2, arg3, arg4
        );
      }

      /// <summary>
      /// Call a static method with 5 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5)
          }),
          arg1, arg2, arg3, arg4, arg5
        );
      }

      /// <summary>
      /// Call a static method with 6 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6
        );
      }

      /// <summary>
      /// Call a static method with 7 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7
        );
      }

      /// <summary>
      /// Call a static method with 8 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8
        );
      }

      /// <summary>
      /// Call a static method with 9 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9
        );
      }

      /// <summary>
      /// Call a static method with 10 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10
        );
      }

      /// <summary>
      /// Call a static method with 11 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11
        );
      }

      /// <summary>
      /// Call a static method with 12 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12
        );
      }

      /// <summary>
      /// Call a static method with 13 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13
        );
      }

      /// <summary>
      /// Call a static method with 14 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14)
          }),
           arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14
         );
      }

      /// <summary>
      /// Call a static method with 15 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15
        );
      }

      /// <summary>
      /// Call a static method with 16 parameters, and has a void return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15),typeof(T16)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16
        );
      }

      /// <summary>
      /// Call a static method with 0 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<TResult>(string FullClassName, string MethodName, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[0]),
          out result
        );
      }

      /// <summary>
      /// Call a static method with 1 parameter, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, TResult>(string FullClassName, string MethodName, T1 arg1, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1)
          }),
          arg1,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 2 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, out TResult result) {
        this.Call(
            this.getMethodInfo(FullClassName, MethodName, new Type[] {
          typeof(T1),typeof(T2)
            }),
            arg1, arg2,
            out result
          );
      }

      /// <summary>
      /// Call a static method with 3 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3)
          }),
          arg1, arg2, arg3,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 4 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4)
          }),
          arg1, arg2, arg3, arg4,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 5 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5)
          }),
          arg1, arg2, arg3, arg4, arg5,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 6 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 7 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 8 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 9 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 10 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 11 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 12 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 13 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 14 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14)
          }),
           arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
           out result
         );
      }

      /// <summary>
      /// Call a static method with 15 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          out result
        );
      }

      /// <summary>
      /// Call a static method with 16 parameters, and has a specified return type.
      /// </summary>
      /// <param name="FullClassName">The full class name including any namespaces.</param>
      /// <param name="MethodName">The name of the static method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(string FullClassName, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out TResult result) {
        this.Call(
          this.getMethodInfo(FullClassName, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15),typeof(T16)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 0 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call(object Instance, string MethodName) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[0])
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 1 parameter and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1>(object Instance, string MethodName, T1 arg1) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1)
          }),
          arg1
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 2 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2>(object Instance, string MethodName, T1 arg1, T2 arg2) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2)
          }),
          arg1, arg2
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 3 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3)
          }),
          arg1, arg2, arg3
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 4 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4)
          }),
          arg1, arg2, arg3, arg4
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 5 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5)
          }),
          arg1, arg2, arg3, arg4, arg5
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 6 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 7 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 8 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 9 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 10 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 11 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 12 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 13 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 14 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14)
          }),
           arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14
         );
      }

      /// <summary>
      /// Call a method from a specified instance with 15 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 16 parameters and has a void return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15),typeof(T16)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 0 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<TResult>(object Instance, string MethodName, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[0]),
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 1 parameter and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, TResult>(object Instance, string MethodName, T1 arg1, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1)
          }),
          arg1,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 2 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2)
          }),
          arg1, arg2,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 3 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3)
          }),
          arg1, arg2, arg3,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 4 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4)
          }),
          arg1, arg2, arg3, arg4,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 5 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5)
          }),
          arg1, arg2, arg3, arg4, arg5,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 6 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 7 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 8 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 9 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 10 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 11 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 12 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 13 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 14 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14)
          }),
           arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
           out result
         );
      }

      /// <summary>
      /// Call a method from a specified instance with 15 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          out result
        );
      }

      /// <summary>
      /// Call a method from a specified instance with 16 parameters and has a specified return type.
      /// </summary>
      /// <param name="Instance">The object instance to call the method from.</param>
      /// <param name="MethodName">The name of the method to call.</param>
      /// <param name="result">out: The return value from the call.</param>
      public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(object Instance, string MethodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out TResult result) {
        this.Call(
          this.getMethodInfo(Instance, MethodName, new Type[] {
            typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6),typeof(T7),typeof(T8),typeof(T9),typeof(T10),typeof(T11),typeof(T12),typeof(T13),typeof(T14),typeof(T15),typeof(T16)
          }),
          arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16,
          out result
        );
      }


      private void Call(MethodInfo mi) {
        var call = (Action)Delegate.CreateDelegate(typeof(Action), mi);
        if(call != null) {
          call();
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1>(MethodInfo mi, T1 arg1) {
        var call = (Action<T1>)Delegate.CreateDelegate(
          typeof(Action<T1>),
          mi
        );
        if(call != null) {
          call(arg1);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2>(MethodInfo mi, T1 arg1, T2 arg2) {
        var call = (Action<T1, T2>)Delegate.CreateDelegate(
          typeof(Action<T1, T2>),
          mi
        );
        if(call != null) {
          call(arg1, arg2);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3) {
        var call = (Action<T1, T2, T3>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        var call = (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
        var call = (Action<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
        var call = (Action<T1, T2, T3, T4, T5, T6>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7>),
         mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>),
         mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
        var call = (Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>)Delegate.CreateDelegate(
          typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>),
          mi
        );
        if(call != null) {
          call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }

      private void Call<TResult>(MethodInfo mi, out TResult result) {
        var call = (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), mi);
        if(call != null) {
          result = call();
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, TResult>(MethodInfo mi, T1 arg1, out TResult result) {
        var call = (Func<T1, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, TResult>(MethodInfo mi, T1 arg1, T2 arg2, out TResult result) {
        var call = (Func<T1, T2, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, out TResult result) {
        var call = (Func<T1, T2, T3, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TResult result) {
        var call = (Func<T1, T2, T3, T4, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
      private void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }

      private void Call<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MethodInfo mi, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out TResult result) {
        var call = (Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>)Delegate.CreateDelegate(
          typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>),
          mi
        );
        if(call != null) {
          result = call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        } else {
          throw new InvalidOperationException("Not able to build delegate!");
        }
      }
    }

  }
}
