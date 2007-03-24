# -*- coding: utf-8 -*-

__all__ = ['AmpleeError', 'UnsupportedMediaType',
           'UnknownResource', 'MemberMediaError',
           'FixedCategoriesError','ResourceOperationException']

class AmpleeError(StandardError):
    pass

class UnsupportedMediaType(AmpleeError):
    pass

class UnknownResource(AmpleeError):
    pass

class MemberMediaError(AmpleeError):
    pass

class FixedCategoriesError(AmpleeError):
    pass

class ResourceOperationException(AmpleeError):
    def __init__(self, msg, code=400):
        self.msg = msg
        self.code = code

    def __str__(self):
        return self.msg
