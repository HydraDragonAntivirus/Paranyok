@echo off
makecert -r -pe -n "CN=Emirhan Ucan" -a sha256 -cy end -sky signature -eku 1.3.6.1.5.5.7.3.3,1.3.6.1.4.1.311.10.3.6 -len 512 -sv paranyok.pvk paranyok.cer 