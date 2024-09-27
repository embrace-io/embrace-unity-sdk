This is where we build out the Embrace iOS Interface.

Run build_xcframework.sh to automatically build the xcframework for the Unity SDK.

Open the project in XCode to edit and expose files. To expose functions to unity, you will need to create a public function decorated with @_cdecl("function_name").

As a result of this, we can only pass primitives back and forth.

To find the currently supported version of the iOS SDK, you can call `Embrace_iOS.GetSDKVersion()` from Unity.