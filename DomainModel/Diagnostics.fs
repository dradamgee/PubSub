module Diagnostics

open System.Diagnostics

type InboxWatcher = 
    static member Watch<'T>(inbox: MailboxProcessor<'T>) = 
        if inbox.CurrentQueueLength > 1000 then Debug.WriteLine(typedefof<'T>.ToString() + " " + inbox.CurrentQueueLength.ToString())

