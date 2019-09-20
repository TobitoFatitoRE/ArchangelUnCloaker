using System.Diagnostics;
using System.Reflection;
using Harmony;

namespace ArchangelUnCloaker{
    internal static class Harmony{
        public static void Patch(){
            var h = HarmonyInstance.Create("tobito.fatito");
            h.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(StackFrame), "GetMethod")]
        public class PatchStackTraceGetMethod{
            public static MethodInfo MethodToReplace;

            public static void Postfix(ref MethodBase __result){
                if (__result.Name.Contains("Invoke"))
                    //just replace it with a method
                    __result = MethodToReplace ?? MethodBase.GetCurrentMethod();
            }
        }

        [HarmonyPatch(typeof(Assembly), "GetCallingAssembly")]
        public class PatchGetCallingAssembly{
            public static Assembly MethodToReplace;

            public static void Postfix(ref Assembly __result){
                if (__result == typeof(PatchGetCallingAssembly).Assembly)
                    //just replace it with a method
                    __result = MethodToReplace;
            }
        }
    }
}