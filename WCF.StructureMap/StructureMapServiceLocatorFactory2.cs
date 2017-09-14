using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wcf.StructureMap
{
    /**
     * https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-define-a-generic-method-with-reflection-emit
     */
    public static class StructureMapServiceLocatorFactory2
    {
        static readonly Type IContainer_Type = typeof(global::StructureMap.IContainer);
        static readonly ConcurrentDictionary<Type, Type> _types = new ConcurrentDictionary<Type, Type>();

        public static Type CreateType(Type intf)
        {
            return _types.GetOrAdd(intf, (type) => {
                var tb = CreateTypeBuilder();
                var containerFld = tb.DefineField("container", typeof(global::StructureMap.IContainer), FieldAttributes.Private);

                tb.AddInterfaceImplementation(intf);

                DefineCopntsructor(tb, containerFld);
                DefineMethod("GetService", tb, containerFld);
                DefineMethod("GetServices", tb, containerFld);

                return tb.CreateType();
            });
        }

        static TypeBuilder CreateTypeBuilder()
        {
            var typeSignature = "ServiceLocator";
            var an = new AssemblyName(typeSignature + "_" + Guid.NewGuid().ToString("N"));
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run/*AndSave*/);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            return moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
        }
        static void DefineCopntsructor(TypeBuilder tb, FieldBuilder containerFld)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var c = tb.DefineConstructor(attrs, CallingConventions.Standard, new[] { IContainer_Type });

            var il = c.GetILGenerator();

            //il.Emit(OpCodes.Ldarg_0);
            //var defaultConstructor = typeof(object).GetConstructors().Where(f => f.GetParameters().Length == 0).FirstOrDefault();
            //il.Emit(OpCodes.Call, defaultConstructor);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, containerFld);
            il.Emit(OpCodes.Ret);

            /*
             * IL_0000: ldarg.0
             * IL_0001: call instance void [mscorlib]System.Object::.ctor()
             * IL_0006: nop
             * IL_0007: nop
             * IL_0008: ldarg.0
             * IL_0009: ldarg.1
             * IL_000a: stfld class [StructureMap]StructureMap.IContainer Wcf.StructureMap.ServiceLocator::container
             * IL_000f: ret
             */
        }

        static void DefineMethod(string name, TypeBuilder tb, FieldBuilder containerFld)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var getService = name == "GetService";
            var m = tb.DefineMethod(name, attrs);
            var ps = m.DefineGenericParameters(new [] { "T" });

            /**
             * var p = ps[0];
             * p.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint| GenericParameterAttributes.DefaultConstructorConstraint)
             * Type icoll = typeof(ICollection<>);
             * Type icollOfTInput = icoll.MakeGenericType(TInput);
             * Type[] constraints = {icollOfTInput};
             * TOutput.SetInterfaceConstraints(constraints);
             */

            m.SetReturnType(getService ? ps[0] : typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(new [] { ps[0] }));

            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, containerFld);

            var IContainer_method_name = getService ? "TryGetInstance" /*GetInstance*/ : "GetAllInstances";
            var methods = IContainer_Type.GetMethods();
            var IContainer_MethodInfo = methods.Where(o =>
            {
                return o.Name == IContainer_method_name && o.IsGenericMethod && o.GetParameters().Length == 0;
            }).FirstOrDefault();

            var gm = IContainer_MethodInfo.MakeGenericMethod(new Type[] { ps[0] });

            il.Emit(OpCodes.Callvirt, gm);
            il.Emit(OpCodes.Ret);

            /*
                IL_0000: nop
                IL_0001: ldarg.0
                IL_0002: ldfld class [StructureMap]StructureMap.IContainer Wcf.StructureMap.ServiceLocator::container
                IL_0007: callvirt instance !!0 [StructureMap]StructureMap.IContainer::GetInstance<!!T>()
                IL_000c: stloc.0
                IL_000d: br.s IL_000f

                IL_000f: ldloc.0
                IL_0010: ret
             */
        }
    }
}