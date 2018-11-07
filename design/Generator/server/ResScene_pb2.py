# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: ResScene.proto

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
  name='ResScene.proto',
  package='',
  serialized_pb=_b('\n\x0eResScene.proto\"i\n\x08ResScene\x12\n\n\x02id\x18\x02 \x02(\x05\x12\x10\n\x08\x61\x63tor_id\x18\x03 \x02(\x05\x12\r\n\x05pos_x\x18\x04 \x02(\x05\x12\r\n\x05pos_z\x18\x05 \x02(\x05\x12\r\n\x05range\x18\x06 \x02(\t\x12\x12\n\nreset_time\x18\x07 \x02(\x02\"*\n\x0eResScene_ARRAY\x12\x18\n\x05items\x18\x01 \x03(\x0b\x32\t.ResScene')
)
_sym_db.RegisterFileDescriptor(DESCRIPTOR)




_RESSCENE = _descriptor.Descriptor(
  name='ResScene',
  full_name='ResScene',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='id', full_name='ResScene.id', index=0,
      number=2, type=5, cpp_type=1, label=2,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='actor_id', full_name='ResScene.actor_id', index=1,
      number=3, type=5, cpp_type=1, label=2,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='pos_x', full_name='ResScene.pos_x', index=2,
      number=4, type=5, cpp_type=1, label=2,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='pos_z', full_name='ResScene.pos_z', index=3,
      number=5, type=5, cpp_type=1, label=2,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='range', full_name='ResScene.range', index=4,
      number=6, type=9, cpp_type=9, label=2,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None),
    _descriptor.FieldDescriptor(
      name='reset_time', full_name='ResScene.reset_time', index=5,
      number=7, type=2, cpp_type=6, label=2,
      has_default_value=False, default_value=0,
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
  serialized_start=18,
  serialized_end=123,
)


_RESSCENE_ARRAY = _descriptor.Descriptor(
  name='ResScene_ARRAY',
  full_name='ResScene_ARRAY',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='items', full_name='ResScene_ARRAY.items', index=0,
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
  serialized_start=125,
  serialized_end=167,
)

_RESSCENE_ARRAY.fields_by_name['items'].message_type = _RESSCENE
DESCRIPTOR.message_types_by_name['ResScene'] = _RESSCENE
DESCRIPTOR.message_types_by_name['ResScene_ARRAY'] = _RESSCENE_ARRAY

ResScene = _reflection.GeneratedProtocolMessageType('ResScene', (_message.Message,), dict(
  DESCRIPTOR = _RESSCENE,
  __module__ = 'ResScene_pb2'
  # @@protoc_insertion_point(class_scope:ResScene)
  ))
_sym_db.RegisterMessage(ResScene)

ResScene_ARRAY = _reflection.GeneratedProtocolMessageType('ResScene_ARRAY', (_message.Message,), dict(
  DESCRIPTOR = _RESSCENE_ARRAY,
  __module__ = 'ResScene_pb2'
  # @@protoc_insertion_point(class_scope:ResScene_ARRAY)
  ))
_sym_db.RegisterMessage(ResScene_ARRAY)


# @@protoc_insertion_point(module_scope)
