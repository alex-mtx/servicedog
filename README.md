# servicedog
A service that helps monitoring Windows based applications, specially .net.

<h2>The need</h2>
In an environment full of application tiers, where distributed applications talk to each oder almost in a streamlined way, it is indispensable having a tool that helps to monitor all these integrations.
For that matter we can count on an <a href="https://en.wikipedia.org/wiki/Application_performance_management" target="_blank">Application Performance Management - APM</a>. We have very good APM choices out there on Internet but unfortunately, for .net running in Windows, one must to pay very high costs for licences. That is why I decided to build servicedog. It will be free and opensource. Forever.

<h2>What does its name mean?</h2>
Let's start with an official service dog definition in the USA:
<blockquote cite="http://www.ada.gov/service_animals_2010.htm">
Service animals are defined as dogs that are individually trained to do work or perform tasks for people with disabilities. Examples of such work or tasks include guiding people who are blind, alerting people who are deaf, pulling a wheelchair, alerting and protecting a person who is having a seizure, reminding a person with mental illness to take prescribed medications, calming a person with Post Traumatic Stress Disorder (PTSD) during an anxiety attack, or performing other duties...
</blockquote>
Notice how this is paired with our sytems environment: usually systems have a perception that something is wrong, normally they record the problem, but they are rarely designed to adequately address such problems. 
Our systems have some disabilities that someone else needs to address, just like a service dog does. Sometimes he just need to warn a medical center, call someone's attention or take some medicine to it´s "patient".

<h2>Goals</h2>
<ul>
<li>near realtime event capturing and notifying.
<li>servicing .net and native applications.
<li>keeping it simple. Yes, KISS.
<li>being robust. 10.000 notifications per second should demand at max 5% of one CPU.
<li>being completely independent of serviced systems.
<li>having some sort of notification plugins infrastructure, so other people can publish events everywhere.
<li>initially perceiving integration problems (on DNS lookups, .net HTTP and WCF clients) and notifying another tool through RabbitMQ.
<li>must work on Windows Server 2008 and greater.
</ul>

<h2>Infrastructure</h2>
Gathering such problems is totally based on <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb968803%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396" target="_blank">Event Tracing Windows (ETW)</a> infrastructure and dependent on <a href="https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent" target="_blank">Microsoft TraceEvent Library</a>.
