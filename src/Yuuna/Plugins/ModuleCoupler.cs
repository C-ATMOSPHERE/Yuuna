﻿// Author: Yuuna-Project@Orlys
// Github: github.com/Orlys
// Contact: orlys@yuuna-project.com

namespace Yuuna
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Yuuna.Contracts.Modules;
    using Yuuna.Contracts.Patterns;
    using Yuuna.Contracts.Semantics;
    using Yuuna.Contracts.TextSegmention;

    public sealed class ModuleCoupler : IDisposable
    {
        private CollectibleLoader _loader;
        private object _lock = new object();
        private ModuleBase _module;
        public Guid Id { get; }

        public IModuleMetadata Metadata { get; }

        public IRule Patterns { get; private set; }

        internal ModuleCoupler(IEnumerable<FileInfo> deps, IEnumerable<FileInfo> dlls)
        {
            this._loader = new CollectibleLoader(deps.First().FullName);
            lock (this._lock)
            {
                foreach (var dll in dlls)
                {
                    try
                    {
                        using (var fs = dll.OpenRead())
                        {
                            var asm = this._loader.LoadFromStream(fs);
                            // error:
                            foreach (var t in asm.GetTypes())
                            {
                                if (t.IsSubclassOf(typeof(ModuleBase)))
                                {
                                    var inst = Activator.CreateInstance(t) as ModuleBase;

                                    this._module = (inst);
                                    this.Metadata = this._module.Metadata;
                                    this.Id = this._module.Id;

                                    break;
                                }
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException reflectEx)
                    {
                        Console.WriteLine(reflectEx.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public void Dispose()
        {
            this._loader.Unload();
            this._loader = null;
        }

        public bool Initialize(ITextSegmenter s, IGroupManager g)
        {
            if (this._module is null)
                return false;

            this._module.Initialize(s, g, out var v);
            this.Patterns = v;
            return true;
        }
    }

    //public sealed class ModuleCoupler : IDisposable
    //{
    //    private WeakReference<ModuleBase> _module;
    //    private CollectibleLoader _loader;
    //    private object _lock = new object();

    // internal ModuleCoupler(IEnumerable<FileInfo> deps, IEnumerable<FileInfo> dlls) { this._loader
    // = new CollectibleLoader(deps.First().FullName); lock (this._lock) { foreach (var dll in dlls)
    // { try { using (var fs = dll.OpenRead()) { var asm = this._loader.LoadFromStream(fs); //
    // error: foreach (var t in asm.GetTypes()) { if (t.IsSubclassOf(typeof(ModuleBase))) { var inst
    // = Activator.CreateInstance(t) as ModuleBase;

    // this._module = new WeakReference<ModuleBase>(inst); if (this._module.TryGetTarget(out var m))
    // { this.Metadata = m.Metadata; this.Id = m.Id;

    // } break; } }

    // } } catch (Exception e) { Console.WriteLine(e); } } } }

    // public Guid Id { get; }

    // public IPatternSet Patterns { get; private set; } public IModuleMetadata Metadata { get; }

    // public bool Initialize(ITextSegmenter s, IGroupManager g) { if (this._module is null) return false;

    // if (this._module.TryGetTarget(out var m)) { m.Initialize(s, g, out var v); this.Patterns = v;
    // return true; } else { Console.WriteLine("???????? "); }

    // return false; }

    //    public void Dispose()
    //    {
    //        this._loader.Unload();
    //        this._loader = null;
    //    }
    //}
}