# Fsky.Term

Hopefully a bluesky terminal client written in F#

# Configurable Options

- DeepLink - A link of a supported page to initialize the application
  - `fsky.term fsky:///login?username=admin`
  - `fsky.term --DeepLink "fsky:///login?username=admin"`
- PdsInstance - Where do you want to connect/authenticate?
  - `fsky.term --PdsInstance "https://my-pds.instance"`
