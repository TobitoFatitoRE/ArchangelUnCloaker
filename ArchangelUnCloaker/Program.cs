using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace ArchangelUnCloaker{
    internal class Program{
        public static void Main(string[] args){
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Title = "Archangel UnCloacker";
            if (args.Length == 0){
                Console.WriteLine("This is a Drag And Drop Program! Exiting.");
                Console.ReadLine();
                return;
            }

            var Module = ModuleDefMD.Load(args[0]);
            var asm = Assembly.LoadFile(args[0]);


            var Asm = Module.GetAssemblyRefs().First(q => q.FullName.Contains("Archangel.CloakingDevice"));
            if (Asm == null) return;

            var asm2 =
                Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(args[0]), "Archangel.CloakingDevice.dll"));
            Harmony.PatchGetCallingAssembly.MethodToReplace = asm;
            Harmony.Patch();

            var invokemethod = asm2.GetType("Archangel.CloakingDevice.KickStart").GetMethod("Boot");
            invokemethod.Invoke(null, null);
            TypeDef specific = null;

            var MethodFields = new Dictionary<FieldDef, MethodDef>();
            foreach (var type in Module.Types)
                if (type.HasFields)
                    foreach (var field in type.Fields)
                        try{
                            var val = asm.ManifestModule.ResolveField(field.MDToken.ToInt32());
                            var value = val.GetValue(null);
                            if (!(value is MulticastDelegate)) continue;

                            var t = (MulticastDelegate) value;
                            var reader = new DynamicMethodBodyReader(Module, t.Method);
                            reader.Read();
                            var def = reader.GetMethod();
                            MethodFields.Add(field, def);
                            specific = type;
                        }
                        catch{
                        }
            Console.WriteLine($"Found {MethodFields.Count} obfuscated fields and deobfuscated them.");

            int replacedmethods = 0;
            foreach (var type in Module.Types)
            foreach (var method in type.Methods){
                if (method.HasBody && method.Body.HasInstructions)
                    foreach (var instr in method.Body.Instructions)
                        if (instr.OpCode == OpCodes.Ldsfld)
                            try{
                                var op = (FieldDef) instr.Operand;
                                var d = MethodFields[op];
                                if (d == null)
                                    continue;
                                method.FreeMethodBody();
                                method.Body = d.Body;
                                replacedmethods++;
                                goto GetOut;
                            }
                            catch{
                            }

                GetOut: ;
            }
            Console.WriteLine($"Successfully Replaced {replacedmethods} out of {MethodFields.Count} methods.");
            if (replacedmethods != MethodFields.Count){
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not all methods were decrypted, rereferencing dll.");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Importer importer = new Importer(Module);
                IMethod Method;
                Method = importer.Import(invokemethod);
            //    method.Body.Instructions.Add(new Instruction(opc,method.Module.Import(Method)));
            var mainmdtok = asm.EntryPoint.MetadataToken;
            foreach (var type in Module.Types){
                foreach (var method in type.Methods){
                    if (method.MDToken.ToInt32() == mainmdtok){
                        method.Body.Instructions.Insert(0,new Instruction(OpCodes.Call,Module.Import(Method)));
                    }
                }
            }
            }
            






            // Module.Types.Remove(specific); Maybe shouldn't remove, you decide.
            Save(Module, args[0]);
            Console.WriteLine("Successfully Saved!");
            Console.ReadLine();

        }

        public static void Save(ModuleDefMD Module, string path){
            var nativeModuleWriterOptions = new ModuleWriterOptions(Module);
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
            var otherstrteams = Module.Metadata.AllStreams.Where(a => a.GetType() == typeof(DotNetStream));
            nativeModuleWriterOptions.MetadataOptions.PreserveHeapOrder(Module, true);
            Module.Write(
                Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + "-UnClocked.exe",
                nativeModuleWriterOptions);
        }
    }
}