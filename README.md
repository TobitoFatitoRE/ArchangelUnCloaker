# ArchangelUnCloaker
  The First Public Open-Sourced ArchangelCloak Deobfuscator
## How to use:
  Drag n Drop obfuscated program on the deobfuscator exe.
## Careful! This can in theory invoke viruses, since it invokes the dll. Always run on sandbox/vm.
 
 Special thanks to holly for his harmony class :D
 
 ## Doesn't Work
  If it doesn't work put the files on the same directory.
  Still doesn't work? The file you are trying to deobfuscate has been renamed
  find the exact assembly name that it had before it was renamed.
 
 ## How does it work
It works by invoking boot method on dll (with harmony attached), then getting the value of the fields and
using DynamicMethodBodyReader it gets the methoddef out of the delegates.
 
