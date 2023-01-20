# file-distributor

Little tool to help distribute files across folders. Keeps the latest (according to the modification time of files) files to fill folder A to the specified size and putting the rest into folder B.

## Usage

```cmd
file-distributor [options]
```

### Required Options

| Short | Long | Descripion |
| --- | --- | --- |
| `-a` | `--folder-a` | Path for folder A |
| `-b` | `--folder-b` | Path for folder B |
| `-s` | `--size` | Maximum size for folder a |

### Optional Options

| Short | Long | Description |
| --- | --- | --- |
| `-m` | `--monitor` | Monitor mode |
| `-i` | `--ignore-keyword` | Specify a keyword to ignore (Can be used multiple times) |
| `-h` | `--help` | Diplay the help page |
