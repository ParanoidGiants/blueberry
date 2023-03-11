namespace Assets.Window
{
    public class Noise     
    {
        public string text = "";
        public string soundGroup = "";

        public Noise(string text, string soundGroup)
        {
            this.text = text;
            this.soundGroup = soundGroup;
        }

        public override string ToString()
        {
            string str = "";
            str += "Text: " + text + "\n";
            str += "SoundGroup: " + soundGroup;
            return str;
        }
    }
}
