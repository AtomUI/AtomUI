# Conditional registration input binding

This directory contains source-only input definitions, input snapshot validation and Cecil schema,
identity/forwarding and collector-template validation. Tool projects explicitly link these sources;
there is no project or runtime assembly, and the Build.Tasks worker does not acquire a Cecil dependency.

The binder receives assembly resolution and actual input-location delegates. It verifies complete
implementation identities and content hashes before accepting the caller's Cecil objects. Results
contain validated declarations and Cecil entities, never linker marking or code-generation state.

Framework-reference rebinding is opt-in and defaults to an empty mapping. The .NET8 input adapter keeps
strict identity resolution. The net10 bridge may provide explicit source-full-identity to target-full-
identity entries only after checking them against the SDK-resolved target FrameworkReference runtime
pack, its managed-file manifest, public key token and actual input hash. Third-party assemblies never
receive simple-name/version fallback. Source/target identities and runtime-pack evidence belong in the
bridge report so normal framework binding is distinguishable from an identity mismatch.

SDK input transport format 2 requires `additionalInputs`, with absolute frozen paths, lowercase
SHA-256 values and a bounded kind vocabulary. Every additional file is read and verified before
binding; duplicate path/kind pairs and aliases of the input/report files are rejected. Format 1
remains only for mechanism experiments, not SDK production input validation.
