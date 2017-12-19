# SlothBot Bridge
Run your existing app / script in a secure fashion without any code

## The User Story
So you have an asset you want to expose over Slack, but dont want to compile anything custom?

>_In this scenario, lets say im an Admin who has a script that he runs to check production servers for uptime. The admin wants to setup some >aliases for this script so users dont need to remmber
>the IP address of each production server. He doesnt want to give access to all scripts in his toolbox._

### Steps 
Admin downloads [SlothBot.Bridge](dist/SlothBot.Bridge-1.0.0.0beta.zip) and creates a ```enabledProcesses.json``` with his tool of choice, adds api key and runs the bridge running under a restricted user

> **Edit** _enabledProcesses.json_
```json
[
  {
      "ProcessName": "verifyAccess",
      "Path": "c:\utils\verifyAccess.exe",
      "UsernamesWhoCanAlias": ["Admin"]
  },
  ... more processes ...
]
```

> **Add API Key** _slothbot.json_
```json
{
  "SlackApiKey": "my-key-from-slack-website"
}
```
_[Read more about configuration](QUICKSTART.md)_

### _SlothBot Bridge_ is now running...
```chat
[Admin]: alias verifyAccess as VerifyProd1AccessForUsers
[SlothBot]: Do you want to add some arguments?
[Admin]: Yes
[SlothBot]: Whats the argument name?
[Admin]: IP
[SlothBot]: Where do you want the argument to come from? Defined, a file, or the user?
[Admin]: defined
[SlothBot]: Whats the value then?
[Admin]: 10.0.0.1
...
[SlothBot]: Ready to use VerifyProd1Uptime
```

> ... later a support email comes in with a user complaining production is unresponsive
```
[Support]:@slothBot help
[SlothBot]: VerifyProd1AccessForUsers: Pings production looking for trouble. Responses should be under 2ms otherwise users may get angry.
...
[Support]: VerifyProd1AccessForUsers
[SlothBot]: Give me a moment...
[ProductionUptime]: Success in 1.5ms
```
> _Support then emails back user: works on my box!_

### but sometimes you want flexibility
bridge allows the user to slightly customise the process params at runtime

```
[Me] : promoteBuild dev to test
[SlothBot] : Give me a second...
[promoteBuild] Zipping files
[promoteBuild] Uploading files..
[promoteBuild] Restarting server ...
[promoteBuild] Done!
```

### but dont want to expose sensitive keys to the world
using a file argument instead of a defined param means the key never touches slack

> slothbot.json_
```json
{
  "IsSecure": true,
  "WorkingDirectory": "./safePrograms"
}
```

```
[SlothBot]: Where do you want the argument to come from? Defined, a file, or the user?
[Me]: file
[SlothBot]: What is the path to the file?
[Me]: c:\keys\key.scp
[SlothBot]: Ok!
...
```

### SlothBot speaks your language...
Talking with _SlothBot_ should be more like talking with a friend. you dont have to be precise
> **_future_**  He will understand your slang terms and look in your sentances for commands 

### ... and lastly, is my tool already bridged?!
bridge current supports apps of type
- [x] any .exe, .bat, .sh
- [ ] method from a .dll

> **Comming soon** pre-packaged configuration that is setup for bridging certain tools 
> * TFS
> * OcotopusDeploy
