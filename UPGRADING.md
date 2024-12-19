# Upgrade guide

# Upgrading from 1.17.0 to 1.18.0

Version 1.18.0 of the Embrace Unity SDK renames some functions. This has been done to reduce
confusion & increase consistency across our SDKs.

Functions that have been marked as deprecated will still work as before, but will be removed in
the next major version release. Please upgrade when convenient, and get in touch if you have a
use-case that isn’t supported by the new API.

| Old API                              | New API                                 | Comments                         |
|--------------------------------------|-----------------------------------------|----------------------------------|
| `Embrace.Instance.SetUserPersona`    | `Embrace.Instance.AddUserPersona`       | Renamed function for consistency |
| `Embrace.Instance.LogBreadcrumb`     | `Embrace.Instance.AddBreadcrumb`        | Renamed function for consistency |
| `Embrace.Instance.LogNetworkRequest` | `Embrace.Instance.RecordNetworkRequest` | Renamed function for consistency |
