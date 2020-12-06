using System.ComponentModel;
using System.Runtime.CompilerServices;
using MonoTorrent;
using Qurrent.Annotations;

namespace Qurrent.Models
{
    class FileModel : INotifyPropertyChanged
    {
        TorrentFile file;

        public string Name => file.Path;
        public string State => file.Priority.ToString().ToLowerInvariant();
        public double Progress => (double)file.BytesDownloaded / file.Length;

        public FileModel(TorrentFile file)
        {
            this.file = file;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshProgress()
        {
            OnPropertyChanged(nameof(Progress));
        }
    }
}
