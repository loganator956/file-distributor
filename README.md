# file-distributor

Little tool to help distribute files across folders. Keeps the latest (according to the modification time of files) files to fill folder A to the specified size and putting the rest into folder B.

## Usage

Download the executable file for your platform and execute it like: 

```cmd
file-distributor [options]
```

May need to add executable permissions to the file.

### Docker

Currently, will need to manually build the Dockerfile using following command.

```bash
docker build -t file-distributor .
```

Then you can run it:

```bash
docker run --name "distribute" \
-v /path/to/folder-a:/folder-a \
-v /mnt/e/test2:/folder-b \
-e FD_SIZE=10 \
localhost/file-distributor
```

Replace the host paths for the volumes with paths to your directories. By default this runs in monitor mode. If you want to disable this, add the `-e FD_MONITOR_MODE=false` argument.

### Required Options

| Short | Long | Environment Variable | Descripion |
| --- | --- | --- | --- |
| `-a` | `--folder-a` | `FD_FOLDER_A` | Path for folder A |
| `-b` | `--folder-b` | `FD_FOLDER_B` | Path for folder B |
| `-s` | `--size` | `FD_SIZE` | Maximum size for folder a |

### Optional Options

| Short | Long | Environment Variable | Description |
| --- | --- | --- | --- |
| `-m` | `--monitor` | `FD_MONITOR_MODE` | Monitor mode |
| `-i` | `--ignore-keyword` | n/a | Specify a keyword to ignore (Can be used multiple times) |
| `-h` | `--help` | n/a | Diplay the help page |
| `-f` | `--ignore-file` | `FD_IGNORE_FILE` | Specifies a path to a file containing ignored keywords. (Each line is a separate keyword. Ignores lines starting with `#`) |
| `-v` | `--verbose` | n/a | Specifies the verbosity level. (Either by calling argument multiple times or specifying a value for argument) |
| `-w` | `--wait-interval` | `FD_MONITOR_WAIT_INTERVAL` | Specifies the amount of time (seconds) between running the distribute files in "monitor" mode |
