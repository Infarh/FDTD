#nullable enable
using System.Runtime.CompilerServices;
// ReSharper disable CheckNamespace

namespace System
{
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int Value, bool FromEnd = false) =>
            _Value = Value < 0
                ? throw new ArgumentOutOfRangeException(nameof(Value))
                : FromEnd
                    ? ~Value
                    : Value;

        private Index(int Value) => _Value = Value;

        public static Index Start => new(0);

        public static Index End => new(-1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value) => value < 0 ? throw new ArgumentOutOfRangeException(nameof(value)) : new Index(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value) => value < 0 ? throw new ArgumentOutOfRangeException(nameof(value)) : new Index(~value);

        public int Value => _Value < 0 ? ~_Value : _Value;

        public bool IsFromEnd => _Value < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int Length) => IsFromEnd ? _Value + Length + 1 : _Value;

        public override bool Equals(object? value) => value is Index index && _Value == index._Value;

        public bool Equals(Index other) => _Value == other._Value;

        public override int GetHashCode() => _Value;

        public static implicit operator Index(int value) => FromStart(value);

        public override string ToString() => /*this.IsFromEnd ? this.ToStringFromEnd() :*/ ((uint)Value).ToString();

//        private unsafe
//#nullable disable
//    string ToStringFromEnd()
//        {
//            // ISSUE: untyped stack allocation
//            Span<char> span = new Span<char>((void*)__untypedstackalloc(new IntPtr(22)), 11);
//            int charsWritten;
//            ((uint)this.Value).TryFormat(span.Slice(1), out charsWritten);
//            span[0] = '^';
//            return new string((ReadOnlySpan<char>)span.Slice(0, charsWritten + 1));
//        }
    }
}
