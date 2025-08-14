using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Mythril.GameLogic
{
    public class SoundManager
    {
        private Dictionary<string, SoundEffect> _sounds;
        private Dictionary<string, Song> _music;
        private ContentManager _content;

        public SoundManager(ContentManager content)
        {
            _content = content;
            _sounds = new Dictionary<string, SoundEffect>();
            _music = new Dictionary<string, Song>();
        }

        public void LoadSound(string soundName, string assetPath)
        {
            // In a real game, you would load the sound effect from the content pipeline
            // For now, we'll just pretend to load it.
            // var sound = _content.Load<SoundEffect>(assetPath);
            // _sounds[soundName] = sound;
        }

        public void PlaySound(string soundName)
        { 
            // if (_sounds.ContainsKey(soundName))
            // {
            //    _sounds[soundName].Play();
            // }
        }

        public void LoadMusic(string musicName, string assetPath)
        {
            // In a real game, you would load the song from the content pipeline
            // For now, we'll just pretend to load it.
            // var song = _content.Load<Song>(assetPath);
            // _music[musicName] = song;
        }

        public void PlayMusic(string musicName)
        {
            // if (_music.ContainsKey(musicName))
            // {
            //     MediaPlayer.Play(_music[musicName]);
            //     MediaPlayer.IsRepeating = true;
            // }
        }
    }
}
