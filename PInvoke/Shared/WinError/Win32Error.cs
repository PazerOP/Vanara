using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Vanara.PInvoke
{
	/// <summary>Represents a Win32 Error Code. This can be used in place of a return value.</summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[TypeConverter(typeof(Win32ErrorTypeConverter))]
	public partial struct Win32Error : IEquatable<Win32Error>, IEquatable<int>, IConvertible, IComparable<Win32Error>, IComparable
	{
		internal readonly int value;

		/// <summary>Initializes a new instance of the <see cref="Win32Error"/> struct with an error value.</summary>
		/// <param name="i">The i.</param>
		public Win32Error(int i) => value = i;

		/// <summary>Gets a value indicating whether this <see cref="Win32Error"/> is a failure.</summary>
		/// <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
		public bool Failed => !Succeeded;

		/// <summary>Gets a value indicating whether this <see cref="Win32Error"/> is a success.</summary>
		/// <value><c>true</c> if succeeded; otherwise, <c>false</c>.</value>
		public bool Succeeded => value == ERROR_SUCCESS;

		/// <summary>Compares the current object with another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value
		/// Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref
		/// name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(Win32Error other) => value.CompareTo(other.value);

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current
		/// instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less
		/// than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the
		/// sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order.
		/// </returns>
		public int CompareTo(object obj)
		{
			var v = ValueFromObj(obj);
			return v.HasValue
				? value.CompareTo(v.Value)
				: throw new ArgumentException(@"Object cannot be converted to a Int32 value for comparison.", nameof(obj));
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool Equals(int other) => other == value;

		/// <summary>Determines whether the specified <see cref="System.Object"/>, is equal to this instance.</summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj) => Equals(value, ValueFromObj(obj));

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool Equals(Win32Error other) => other.value == value;

		/// <summary>Gets the .NET <see cref="Exception"/> associated with the HRESULT value and optionally adds the supplied message.</summary>
		/// <param name="message">The optional message to assign to the <see cref="Exception"/>.</param>
		/// <returns>The associated <see cref="Exception"/> or <c>null</c> if this HRESULT is not a failure.</returns>
		[SecurityCritical, SecuritySafeCritical]
		public Exception GetException(string message = null) => Succeeded ? null : ToHRESULT().GetException(message);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode() => value.GetHashCode();

		/// <summary>Gets the last error.</summary>
		/// <returns></returns>
		[SecurityCritical]
		[System.Diagnostics.DebuggerStepThrough]
		public static Win32Error GetLastError() => new Win32Error(Marshal.GetLastWin32Error());

		/// <summary>Throws the last error.</summary>
		/// <param name="message">The message to associate with the exception.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void ThrowLastError(string message = null) => GetLastError().ThrowIfFailed(message);

		/// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public override string ToString()
		{
			foreach (var info2 in typeof(Win32Error).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (info2.FieldType == typeof(int) && (int)info2.GetValue(null) == value)
					return info2.Name;
			}
			return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", value);
		}

		/// <summary>To the hresult.</summary>
		/// <returns></returns>
		public HRESULT ToHRESULT() => (HRESULT)this;

		/// <summary>Throws if failed.</summary>
		/// <param name="message">The message.</param>
		/// <exception cref="Win32Exception"></exception>
		[System.Diagnostics.DebuggerStepThrough]
		public void ThrowIfFailed(string message = null)
		{
			if (value != ERROR_SUCCESS) throw GetException(message);
		}

		/// <summary>Throws if failed.</summary>
		/// <param name="err">The error.</param>
		/// <param name="message">The message.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void ThrowIfFailed(Win32Error err, string message = null) => err.ThrowIfFailed(message);

		/// <summary>Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Win32Error"/>.</summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Win32Error(int value) => new Win32Error(value);

		/// <summary>Performs an explicit conversion from <see cref="Win32Error"/> to <see cref="System.Int32"/>.</summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator int(Win32Error value) => value.value;

		/// <summary>Performs an explicit conversion from <see cref="Win32Error"/> to <see cref="HRESULT"/>.</summary>
		/// <param name="error">The error.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator HRESULT(Win32Error error)
			=>
				error.value <= 0
					? new HRESULT((uint)error.value)
					: HRESULT.Make(true, HRESULT.FacilityCode.FACILITY_WIN32, (uint)error.value & 0xffff);

		/// <summary>Implements the operator ==.</summary>
		/// <param name="errLeft">The error left.</param>
		/// <param name="errRight">The error right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(Win32Error errLeft, Win32Error errRight) => errLeft.value == errRight.value;

		/// <summary>Implements the operator ==.</summary>
		/// <param name="errLeft">The error left.</param>
		/// <param name="errRight">The error right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(Win32Error errLeft, int errRight) => errLeft.value == errRight;

		/// <summary>Implements the operator !=.</summary>
		/// <param name="errLeft">The error left.</param>
		/// <param name="errRight">The error right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(Win32Error errLeft, Win32Error errRight) => errLeft.value != errRight.value;

		/// <summary>Implements the operator !=.</summary>
		/// <param name="errLeft">The error left.</param>
		/// <param name="errRight">The error right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(Win32Error errLeft, int errRight) => errLeft.value != errRight;

		private static int? ValueFromObj(object obj)
		{
			if (obj == null) return null;
			var c = TypeDescriptor.GetConverter(obj);
			return c.CanConvertTo(typeof(int)) ? (int?)c.ConvertTo(obj, typeof(int)) : null;
		}

		TypeCode IConvertible.GetTypeCode() => value.GetTypeCode();

		bool IConvertible.ToBoolean(IFormatProvider provider) => Succeeded;

		char IConvertible.ToChar(IFormatProvider provider) => throw new NotSupportedException();

		sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)value).ToSByte(provider);

		byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)value).ToByte(provider);

		short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)value).ToInt16(provider);

		ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)value).ToUInt16(provider);

		int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)value).ToInt32(provider);

		uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)value).ToUInt32(provider);

		long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)value).ToInt64(provider);

		ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)value).ToUInt64(provider);

		float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)value).ToSingle(provider);

		double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)value).ToDouble(provider);

		decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)value).ToDecimal(provider);

		DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new NotSupportedException();

		string IConvertible.ToString(IFormatProvider provider) => ToString();

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) =>
			((IConvertible)value).ToType(conversionType, provider);
	}

	internal class Win32ErrorTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.IsPrimitive && sourceType != typeof(bool) && sourceType != typeof(char))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string) || destinationType.IsPrimitive && destinationType != typeof(char))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null && value.GetType().IsPrimitive && !(value is char) && !(value is bool))
				return new Win32Error((int)Convert.ChangeType(value, TypeCode.Int32));
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
			Type destinationType)
		{
			if (!(value is Win32Error err)) throw new NotSupportedException();
			if (destinationType.IsPrimitive && destinationType != typeof(char))
				return Convert.ChangeType(err, destinationType);
			if (destinationType == typeof(string))
				return err.ToString();
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}