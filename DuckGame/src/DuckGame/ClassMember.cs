using System;
using System.Reflection;

namespace DuckGame;

public class ClassMember
{
	private FieldInfo _fieldInfo;

	private PropertyInfo _propertyInfo;

	private Type _declaringType;

	private bool _isPrivate;

	private string _name;

	private AccessorInfo _accessor;

	public FieldInfo field => _fieldInfo;

	public PropertyInfo property => _propertyInfo;

	public Type declaringType => _declaringType;

	public bool isPrivate => _isPrivate;

	public string name => _name;

	public Type type
	{
		get
		{
			if (_fieldInfo != null)
			{
				return _fieldInfo.FieldType;
			}
			return _propertyInfo.PropertyType;
		}
	}

	public bool isConst
	{
		get
		{
			if (_fieldInfo != null)
			{
				if (_fieldInfo.IsLiteral)
				{
					return !_fieldInfo.IsInitOnly;
				}
				return false;
			}
			return false;
		}
	}

	public bool hasGetterAndSetter
	{
		get
		{
			if (_accessor == null)
			{
				_accessor = Editor.GetAccessorInfo(_declaringType, _name, _fieldInfo, _propertyInfo);
			}
			if (_accessor.getAccessor != null)
			{
				return _accessor.setAccessor != null;
			}
			return false;
		}
	}

	public object GetValue(object instance)
	{
		if (_accessor == null)
		{
			_accessor = Editor.GetAccessorInfo(_declaringType, _name, _fieldInfo, _propertyInfo);
		}
		return _accessor.getAccessor(instance);
	}

	public void SetValue(object instance, object value)
	{
		if (_accessor == null)
		{
			if (_fieldInfo != null)
			{
				_ = _fieldInfo.Name;
				_ = _fieldInfo.FieldType;
			}
			if (_propertyInfo != null)
			{
				_ = _propertyInfo.Name;
				_ = _propertyInfo.PropertyType;
			}
			_accessor = Editor.GetAccessorInfo(_declaringType, _name, _fieldInfo, _propertyInfo);
		}
		_accessor.setAccessor(instance, value);
	}

	public ClassMember(string n, Type declaringTp, FieldInfo field)
	{
		_fieldInfo = field;
		_name = n;
		_declaringType = declaringTp;
		_isPrivate = field.IsPrivate;
	}

	public ClassMember(string n, Type declaringTp, PropertyInfo property)
	{
		_propertyInfo = property;
		_name = n;
		_declaringType = declaringTp;
	}
}
