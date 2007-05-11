def lookup_keys():
    f = file('awsenv.sh', 'rb')
    pub_key = f.readline().strip()
    secret_key = f.readline().strip()
    #print pub_key, secret_key
    return (pub_key, secret_key)
    
