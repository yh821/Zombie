# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: Condition.proto

import sys
_b=sys.version_info[0]<3 and (lambda x:x) or (lambda x:x.encode('latin1'))
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
from google.protobuf import descriptor_pb2
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()




DESCRIPTOR = _descriptor.FileDescriptor(
  name='Condition.proto',
  package='',
  serialized_pb=_b('\n\x0f\x43ondition.proto\"\xa1\x01\n\tCondition\x12\x13\n\x0b\x63onditionId\x18\x03 \x02(\x05\x12\x11\n\x06target\x18\x04 \x01(\x05:\x01\x30\x12\x13\n\x08occasion\x18\x05 \x01(\x05:\x01\x30\x12\x10\n\x05\x64\x65lay\x18\x06 \x01(\x05:\x01\x30\x12\x12\n\ndamageType\x18\x07 \x03(\x05\x12\x0f\n\x04\x61ttr\x18\x08 \x01(\x05:\x01\x30\x12\x12\n\npercentage\x18\t \x03(\t\x12\x0c\n\x04\x62\x61se\x18\n \x03(\t\",\n\x0f\x43ondition_ARRAY\x12\x19\n\x05items\x18\x01 \x03(\x0b\x32\n.Condition')
)
_sym_db.RegisterFileDescriptor(DESCRIPTOR)




_CONDITION = _descriptor.Descriptor(
  name='Condition',
  full_name='Condition',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='conditionId', full_name='Condition.conditionId', index=0,
      number=3, type=5, cpp_type=1, label=2,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='target', full_name='Condition.target', index=1,
      number=4, type=5, cpp_type=1, label=1,
      has_default_value=True, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='occasion', full_name='Condition.occasion', index=2,
      number=5, type=5, cpp_type=1, label=1,
      has_default_value=True, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='delay', full_name='Condition.delay', index=3,
      number=6, type=5, cpp_type=1, label=1,
      has_default_value=True, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='damageType', full_name='Condition.damageType', index=4,
      number=7, type=5, cpp_type=1, label=3,
      has_default_value=False, default_value=[],
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='attr', full_name='Condition.attr', index=5,
      number=8, type=5, cpp_type=1, label=1,
      has_default_value=True, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='percentage', full_name='Condition.percentage', index=6,
      number=9, type=9, cpp_type=9, label=3,
      has_default_value=False, default_value=[],
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='base', full_name='Condition.base', index=7,
      number=10, type=9, cpp_type=9, label=3,
      has_default_value=False, default_value=[],
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  options=None,
  is_extendable=False,
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=20,
  serialized_end=181,
)


_CONDITION_ARRAY = _descriptor.Descriptor(
  name='Condition_ARRAY',
  full_name='Condition_ARRAY',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='items', full_name='Condition_ARRAY.items', index=0,
      number=1, type=11, cpp_type=10, label=3,
      has_default_value=False, default_value=[],
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  options=None,
  is_extendable=False,
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=183,
  serialized_end=227,
)

_CONDITION_ARRAY.fields_by_name['items'].message_type = _CONDITION
DESCRIPTOR.message_types_by_name['Condition'] = _CONDITION
DESCRIPTOR.message_types_by_name['Condition_ARRAY'] = _CONDITION_ARRAY

Condition = _reflection.GeneratedProtocolMessageType('Condition', (_message.Message,), dict(
  DESCRIPTOR = _CONDITION,
  __module__ = 'Condition_pb2'
  # @@protoc_insertion_point(class_scope:Condition)
  ))
_sym_db.RegisterMessage(Condition)

Condition_ARRAY = _reflection.GeneratedProtocolMessageType('Condition_ARRAY', (_message.Message,), dict(
  DESCRIPTOR = _CONDITION_ARRAY,
  __module__ = 'Condition_pb2'
  # @@protoc_insertion_point(class_scope:Condition_ARRAY)
  ))
_sym_db.RegisterMessage(Condition_ARRAY)


# @@protoc_insertion_point(module_scope)
