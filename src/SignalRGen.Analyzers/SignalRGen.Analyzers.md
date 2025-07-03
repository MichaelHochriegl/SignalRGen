# SignalRGen.Analyzers

## SRG0001: Methods not allowed in HubContract

A HubContract cannot have any methods defined.

| Item     | Value      |
|----------|------------|
| Category | SignalRGen |
| Enabled  | True       |
| Severity | Error      |
| CodeFix  | True       |


## SRG0002: Methods with return type are not allowed in server-to-client communication

An interface used as a server-to-client communication contract can only have `Task` as a return type.

| Item     | Value      |
|----------|------------|
| Category | SignalRGen |
| Enabled  | True       |
| Severity | Error      |
| CodeFix  | True       |


## SRG0003: Methods with non-Task or Task<T> return type are not allowed in client-to-server communication

An interface used as a client-to-server communication contract can only have `Task` or `Task<T>` as a return type.

| Item     | Value      |
|----------|------------|
| Category | SignalRGen |
| Enabled  | True       |
| Severity | Error      |
| CodeFix  | True       |