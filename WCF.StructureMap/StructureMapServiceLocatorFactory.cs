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
    public static class StructureMapServiceLocatorFactory
    {
        static readonly Type ServiceLocator_Type = typeof(StructureMapServiceLocator);
        static readonly ConstructorInfo ServiceLocator_Type_ctor;
        static readonly MethodInfo ServiceLocator_Type_TryGetInstance;
        static readonly MethodInfo ServiceLocator_Type_GetAllInstances;
        static readonly Type IContainer_Type = typeof(global::StructureMap.IContainer);
        static readonly ConcurrentDictionary<Type, Type> _types = new ConcurrentDictionary<Type, Type>();

        static StructureMapServiceLocatorFactory()
        {
            ServiceLocator_Type_ctor = ServiceLocator_Type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(global::StructureMap.IContainer) }, null);
            ServiceLocator_Type_TryGetInstance = ServiceLocator_Type.GetMethod("TryGetInstance", BindingFlags.NonPublic | BindingFlags.Instance); ;
            ServiceLocator_Type_GetAllInstances = ServiceLocator_Type.GetMethod("GetAllInstances", BindingFlags.NonPublic | BindingFlags.Instance); ;
        }

        public static object CreateInstance(Type intf, global::StructureMap.IContainer container)
        {
            if (null != intf)
            {
                var type = CreateType(intf);
                return Activator.CreateInstance(type, new[] { container });
            }
            return null;
        }

        public static Type CreateType(Type intf)
        {
            return _types.GetOrAdd(intf, (type) => {
                var tb = CreateTypeBuilder();

                tb.AddInterfaceImplementation(intf);

                DefineCopntsructor(tb);
                DefineMethod("GetService", tb);
                DefineMethod("GetServices", tb);

                return tb.CreateType();
            });
        }

        static TypeBuilder CreateTypeBuilder()
        {
            var typeSignature = "ServiceLocator";
            var an = new AssemblyName(typeSignature + "_" + Guid.NewGuid().ToString("N"));
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            return moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    ServiceLocator_Type);
        }
        static void DefineCopntsructor(TypeBuilder tb)
        {
            var attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var c = tb.DefineConstructor(attrs, CallingConventions.Standard, new[] { IContainer_Type });

            var il = c.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.Emit(OpCodes.Call, ServiceLocator_Type_ctor);
            il.Emit(OpCodes.Ret);

            /*
             * IL_0000: ldarg.0
             * IL_0001: ldarg.1
             * IL_0002: call instance void WCF.StructureMap.ServiceLocator::.ctor(class [StructureMap]StructureMap.IContainer)
             * IL_0007: nop
             * IL_0008: nop
             * IL_0009: ret
             */
        }

        static void DefineMethod(string name, TypeBuilder tb)
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

            var mth = getService ? 
                ServiceLocator_Type_TryGetInstance : 
                ServiceLocator_Type_GetAllInstances;

            var mth_g = mth.MakeGenericMethod(new Type[] { ps[0] });

            il.Emit(OpCodes.Call, mth_g);
            il.Emit(OpCodes.Ret);

            /*
             * IL_0000: nop
             * IL_0001: ldarg.0
             * IL_0002: call instance !!0 WCF.StructureMap.ServiceLocator::GetInstance<!!T>()
             * IL_0007: stloc.0
             * IL_0008: br.s IL_000a
             * IL_000a: ldloc.0
             * IL_000b: ret
             */
        }
    }
}