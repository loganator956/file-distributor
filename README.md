# file-distributor

Little tool to help distribute files.  
Keeps the latest (modify datetime) *x* GiB files in folder A, puts the rest in folder B.  
For example, I use it to keep the latest 10GiB of photos/videos on my phone and put the rest on its SD Card. (Then run again to keep the latest 50GiB files on the SD card and the rest on my PC storage)

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
