# Windows Gallery specialist

This on-demand native host executes the shipping Gallery configuration and every
registered route. It checks exact view factories, view models, retained page
identity across appearance changes, return navigation and language commands.
It is separate from the global xUnit suite: unit routing tests cannot discover a
generated dispatcher selecting an unintended page only in the published app.

The Windows specialist invokes this executable, verifies its actual process HWND
and absence of CoreCLR in NativeAOT, and cleans its owned outputs. Execution takes
about one minute after compilation. There are no additional global test cases.
The explicit Skia compatibility opt-in matches the shipping Gallery host.
