Lec is a central Let's Encrypt proxy API that handles DNS-01 challenge automatically
--------


Lec is a utility that helps apply domain validation certificates from the open [Let's encrypt](https://letsencrypt.org/) CA using the [DNS-01 challenge](https://letsencrypt.org/how-it-works/) to prove your ownership of the domain name to Let's Encrypt CA, and expose these capabilities as Web APIs.

Lec can be used to apply certificates for non-webserver service endpoints, it also can be used as a centralized certificate management tool for requesting and renewing Let's Encrypt certificates.

## Usage
This tool support all operating systems. There are three forms of redistributions: 

* A command line tool
* A web version
* A docker image containing a web version (Recommended)

For the web version, launch the web application (e.g. http://localhost:5000), and then:
1. Navigate to its url and fill your domain information to register
2. Request a new certificate at any time



For the command line version, use it as follows:
1. Download the latest binaries from the releases;
2. Create a new registration profile if this is your first running;
3. Apply a certificate using your registration profile (Currently, Azure DNS service and DnsPod DNS service are supported, more DNS service providers are on the way. You can also implement your own DNSProvider type.).

Here are the sample commands:

```powershell
.\lec reg --accept-tos --contact user@domain.com --out-reg reg.json --out-signer signer.key
.\lec apply some.domain.com --out cert.pem --out-type pem --reg reg.json --signer signer.key --dns Azure --dns-conf "client_id=1234;client_secret=822321668a;subscription_id=9837549;zone_name=domain.com"
```

## Why Lec
Hate the certbot tools because they need to deploy a script or a daemon job on your web server? Wanted to prevent your webserver from being modified by the ACME operation scripts? Or you have a web server cluster that on which you don't want to deploy the ACME operation scripts? Then Lec is your top choice! 

Setup your DNS on to one of supported DNS services, and let Lec to handle the rest for applying new certifications. With Lec, you don't need to install extra script or daemon job on your web servers, just call a Lec web api at any time to fetch new certificates. Lec will determine if the latest acquired certificate matches your validity requirement and request a new certificate automatically if needed. 


## Contributing and license
Pull requests are welcome. The next step is to support more dynamic DNS providers, Godaddy DNS, AWS Router and Google DNS services.

The source code of this project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
