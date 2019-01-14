# WindowsBrokerInfrastructureBug

Steps for reproducing unreliable task invocation from the Microsoft.Windows.BrokerInfrastructure

On a mobile device...

1. Deploy/run the app. 
2. Stop it.
3. Set to "Always allowed in background" under app battery use
4. Copy the included Sleep.xml to Documents\FieldMedic\CustomProfiles\Sleep.xml on the device and start a trace in FieldMedic with it.
5. Turn off the mobile screen
6. Check the log after 8 hrs

You should see:

```
 12/31 09:14:27: TimerTest registered
 12/31 09:38:49: TimerTest 37 mins elapsed
 12/31 09:55:47: TimerTest 16 mins elapsed
 12/31 10:02:50: TimerTest 7 mins elapsed
 12/31 10:25:46: TimerTest 22 mins elapsed
 12/31 10:40:46: TimerTest 14 mins elapsed
 12/31 13:33:15: TimerTest 172 mins elapsed <--- screen turned on.
 ```

The *.etl trace for the above run is included in the rar in this repository. 

Filter results to "5e4ddee2" (from package id 5e4ddee2-4eef-4362-8107-881b602db5ca_1.0.0.0_arm__d3rsr107h1f3p)
- Microsoft.Windows.ProcessLifetimeManager last event is 10:40 AM
- Microsoft-Broker-Infrastructure has no "energy debt" outputs explaining why it stopped at 10:40 AM (ostensibly due to "Always allowed")

