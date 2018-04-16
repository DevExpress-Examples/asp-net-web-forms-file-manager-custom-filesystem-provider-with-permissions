using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class FileSystemItem {
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsFolder { get; set; }
    public DateTime LastWriteTime { get; set; }
    public long Size { get; set; }
    public byte[] Content { get; set; }
    public FileSystemItem Parent { get; set; }
}

public class Permissions {
    public int Id { get; set; }
    public virtual FileSystemItem Item { get; set; }
    public bool Rename { get; set; }
    public bool Move { get; set; }
    public bool Copy { get; set; }
    public bool Delete { get; set; }
    public bool Download { get; set; }
    public bool Create { get; set; }
    public bool Upload { get; set; }
    public bool MoveOrCopyInto { get; set; }
}