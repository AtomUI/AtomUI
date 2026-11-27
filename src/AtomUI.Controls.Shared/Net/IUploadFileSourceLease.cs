namespace AtomUI.Controls;

/// <summary>
/// An upload file source that its users hold as a lease and release by disposing it;
/// <c>UploadStorageFileSource</c> in AtomUI.Desktop.Controls implements it.
/// </summary>
/// <remarks>
/// The interface adds no member to <see cref="IDisposable.Dispose" />: disposing the lease releases the source it
/// belongs to. The upload control and its batch operation in AtomUI.Desktop.Controls collect the leases they own,
/// each once per reference, and dispose them when the file is removed or the operation ends.
/// </remarks>
internal interface IUploadFileSourceLease : IDisposable
{
}
