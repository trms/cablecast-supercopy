The MIT License
 
Copyright (c) 2008 Brandon McKenzie
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.


Installation:

Place the enclosed files in
	D:\TRMS\Web\Cablecast\Plugins\SuperCopy\

Create a directory called bin in the above folder

Copy TRMS.Components.UserManagement.dll and MediaChase.FileUploader.dll from D:\TRMS\Web\Cablecast\bin to the bin folder you just created.

In the IIS applet, navigate to <Web Site Root>/Cablecast/Web/Plugins, right click on SuperCopy, select Properties

Under 'Application' click [Create].  Click [OK].

In the Services Control Panel, set the "Distributed Transaction Coordinator" service to the startup type of "Automatic", and start it if it hasn't been already.

That should be it!
