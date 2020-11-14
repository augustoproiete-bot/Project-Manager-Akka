using LanguageExt;

namespace AkkaTest
{
    [Union]
    public abstract partial class Shape
    {
        public abstract Shape Rectangle(float width, float length);
        public abstract Shape Circle(float radius);
        public abstract Shape Prism(float width, float height);
    }
}