# -*- coding: utf-8 -*-

__all__ = ['lookup_keys']

def lookup_keys(path):
    f = file(path, 'rb')
    pub_key = f.readline().strip()
    secret_key = f.readline().strip()
    return (pub_key, secret_key)
    
