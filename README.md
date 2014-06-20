LLBLGenProContrib
=================

Library of additional code, to be used with LLBLGen Pro Runtime Framework, or LLBLGen Pro Designer.
This library is a collection of code which wasn't addable to the main LLBLGen Pro Runtime Framework
because of .NET version restrictions (as the LLBLGen Pro Runtime Framework has to run on .NET 3.5). 

Which branch to get
--------------------
The code base has several branches. The 'master' branch is the code base for the latest RTM version of LLBLGen Pro.
The other branches are for the version equal to the name of the branch, so the branch 'v4.1' is for LLBLGen Pro v4.1

Building the code
-------------------
To build the code, you have to have LLBLGen Pro installed as it references the LLBLGen Pro Runtime Framework. You might be able to 
use the code with earlier versions, but these aren't tested/supported. The Contrib library sourcecode is using .NET 4.5. 

Contents
---------
* Caching. This folder contains additional IResultsetCache implementations for 3rd party cache providers, like
  an implementation which utilizes the .NET 4.0 MemoryCache, available in the .NET framework.

License
------------
COPYRIGHTS:
Copyright (c)2002-2014 Solutions Design. All rights reserved.

This LLBLGen Pro Contrib library is released under the following license: (BSD2)

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met: 

1) Redistributions of source code must retain the above copyright notice, this list of 
   conditions and the following disclaimer. 
   
2) Redistributions in binary form must reproduce the above copyright notice, this list of 
   conditions and the following disclaimer in the documentation and/or other materials 
   provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY SOLUTIONS DESIGN ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, 
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL SOLUTIONS DESIGN OR CONTRIBUTORS BE LIABLE FOR 
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 

The views and conclusions contained in the software and documentation are those of the authors 
and should not be interpreted as representing official policies, either expressed or implied, 
of Solutions Design. 