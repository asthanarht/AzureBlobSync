 

1.1   INTRODUCTION 

 

The Microsoft Sync Framework (MSF) is a comprehensive framework for synchronizing offline data with its online counterpart. Using this framework, we can build applications that can synchronize data from any data store using any protocol over any type of network. It is independent of the protocol used and the data store that contains the data to be synchronized.

Microsoft says, “Microsoft Sync Framework is a comprehensive synchronization platform that enables collaboration and offline access for applications, services, and devices. It features technologies and tools that enable roaming, sharing, and taking data offline. Using Microsoft Sync Framework, developers can build sync ecosystems that integrate any application, with any data from any store using any protocol over any network.”

 

1.2   BENEFITS OF SYNCHRONIZATION FRAMEWORK 

 

The sync framework takes care of the following issues which would otherwise require unnecessary work and is not guaranteed to be robust:

 

 

Also some other potential issues would need to be addressed such as:

Storage and application errors and failover handling
Network failure
Conflict detection
 

1.3   SALIENT FEATURES OF SYNC FRAMEWORK 

 

Some of the salient features of the Microsoft Sync Framework are:

 

Extensible provider model
Built-in support for filters and data conflict resolution
 

 

Sync support for file systems, databases, and simple sharing extensions (SSEs) such as Really Simple Syndication (RSS) and Atom feeds
Supports peer-to-peer and hub-and-spoke topologies
Works with any number of end points over any configuration
Can be used from managed as well as unmanaged code
 

1.4   INSTALLING MICROSOFT SYNC FRAMEWORK 

 

You can get a copy of the Microsoft Sync Framework software development kit (SDK) in three ways:

 

Link 
 
 

 

 

 

1.5   CORE COMPONENTS OF THE SYNC FRAMEWORK 

 

1.5.1   Core Components 

 

The following diagram shows how a provider built using Sync Framework communicates with a data source and retrieves state information from a metadata store. These providers in turn communicate with other providers through a synchronization session.

 

 

 

 

 

 

 

 

 

 

 

 

 

1.5.2   Data Source 

 

The data source is the location where all information which needs to be synchronized is stored. A data source could be a relational database, a file, a Web Service or even a custom data source included within a line of business application. As long as we can programmatically access the data, it can participate in synchronization.

 

1.5.3   Metadata 

 

A fundamental component of a provider is the ability to store information about the data store and the objects within that data store with respect to state and change information. Metadata can be stored in a file, within a database or within the data source being synchronized. As an optional convenience, Sync Framework offers a complete implementation of a metadata store built on a lightweight database that runs in our process. The metadata for a data store can be broken down into five key components:

Versions 
Knowledge 
·         Tick count
 

·         Replica ID
Tombstones
For each item that is being synchronized, a small amount of information is stored that describes where and when the item was changed. This metadata is composed of two versions: a creation version and an update version. A version is composed of two components: a tick count assigned by the data store and the replica ID for the data store. As items are updated, the current tick count is applied to that item and the tick count is incremented by the data store. The replica ID is a unique value that identifies a particular data store. The creation version is the same as the update version when the item is created. Subsequent updates to the item modify the update version.

The two primary ways that versioning can be implemented are:

Inline tracking:
 

Asynchronous tracking:
 

All change-tracking must occur at least at the level of items. In other words, every item must have an independent version. In the case of a database, an item might be the entire row within a table. Alternatively, an item might be a column within a row of a table. In the case of file synchronization an item will likely be the file. More granular tracking is highly desirable in some scenarios as it reduces the potential for data conflicts (two users updating the same item on different replicas). The downside is that it increases the amount of change-tracking information stored.

 

 

 

 

Another key concept that we need to discuss is the notion of knowledge. Knowledge is a compact representation of changes that the replica is aware of. As version information is updated so does the knowledge for the data store. Providers use replica knowledge to:

 

Enumerate changes (determine which changes another replica is not aware of).
Detect conflicts (determine which operations were made without knowledge of each other)
 

Each replica must also maintain tombstone information for each of the items that are deleted. This is important because when synchronization is executed, if the item is no longer there, the provider will have no way of telling that this item has been deleted and cannot propagate the change to other providers. A tombstone must contain the following information:

Global ID.
Deletion version.
·        
 

Because the number of tombstones will grow over time, it may be prudent to create a process to clean up this store after a period of time in order to save space. Support for managing tombstone information is provided with Sync Framework.

 

 

 

 

 

 

 

 

 

 

 

 

 

 

1.6   SYNCHRONIZATION FLOW

 

The replica where synchronization is initiated is called the source and the replica it connects to is called the destination. The following sections outline the flow of synchronization described in the following diagram. For bidirectional synchronization, this process will be executed twice; source and destination swapped on the second iteration.

 

 

 

1.6.1   Synchronization Session Initiated With Destination

 

During this phase, the source provider initiates communication to the destination provider. The link between the two providers is called a synchronization session.

 

1.6.2   Destination Prepares & Sends Knowledge

 

As discussed previously, each replica stores its own unique knowledge. The knowledge stored in the destination is passed on to the source.

 

1.6.3   Destination Knowledge Used To Determine Changes To Be Sent

 

On the source side, the knowledge that was just received is compared to the local item versions to determine the items that the destination does not know about. It is important to note that the versions that are sent are not the actual items but a summary of where the last change was made to each item. 

 

1.6.4   Change Versions and Source Knowledge sent to Destination

 

Once the source has prepared the list of change versions required, they are transported to the destination

 

1.6.5   Local Version Retrieved for Change Items and Compared against

Source Version and Knowledge

 

The destination uses the versions to prepare a list of items that the source needs to send. The destination also uses this information to detect if there are any constraint conflicts.

 

1.6.6   Conflicts are Detected and Resolved or Deferred

 

A conflict is detected when the change version in one replica does NOT contain the knowledge of the other. Fundamentally, a conflict occurs if a change is made to the same item on two replicas between synchronization sessions.

Conflicts specifically occur when the source knowledge does not contain the destination version for an item (it is implied that the destination knowledge does not contain any of the source versions sent).

 

If the version is contained in the destination's knowledge then the change is considered obsolete.

Replicas are free to implement a variety of policies for the resolution of items in conflict across the synchronization community. Below are some examples of commonly used resolution policies:

·      
·       Destination Wins: Remote replica always wins
·      
·      
·      
·      
1.6.7   Destination Requests Item Data From Source

 

During this phase the destination has determined which items in the source need to be retrieved and communicates this request to the source.

 

1.6.8   Source Prepares And Sends Item Data

 

The source takes the item data request and prepares the actual data to be transferred to the destination. If the item being tracked is a row in a database, that row will be sent. If the item is a file in a folder then the file will be transferred.

 

1.6.8   Items Are Applied At Destination

 

The items received are taken and applied at the destination. If there are any errors during this process, such as network failure, the items will be tagged as exceptions and corrected during the next synchronization. The knowledge received from the source is added to the destination knowledge.

 

 

 

2.       Blob Sync Provider

 

One of the key features of the Sync Framework is that it can be extended to any kind of files available on any provider. Thus, for syncing files on azure with a directory on the client system, we need to make a provider with methods that enable sync framework to identify and synchronize only the required files. For synchronizing blobs with files present in directories present on local system we would use file synchronization provider. The sync library built for this purpose has the following components:

 

Thus there are two providers acting simultaneously in the application:

 

 

2.1         Why use FullEnumerationSimpleSyncProvider?

 

The basic idea behind a full enumeration synchronization provider is that we need to tell Sync Framework some basic information about the items we'll be synchronizing, how to identify them and how to detect a version change, and then give Sync Framework the ability to enumerate through the items looking for changes. To tell Sync Framework about the items, override the MetadataSchema property of the FullEnumerationSimpleSyncProvider class. When we build the metadata schema we’ll specify a set of custom fields to track, and an IdentityRule. Together these things make up the set of data required to track and identify changes for objects in the store.

 

Next, we would need to specify how to insert, update and delete item to complete the sync operation. This is done by overriding some of the functions of FullEnumerationSimpleSyncProvider that are invoked by the sync framework itself.

 

 

 

 

2.2         Concurrency Issues

 

The sync framework supports optimistic concurrency and it is supported by Azure blob storage as well. In such a type of concurrency check before committing, each transaction verifies that no other transaction has modified its data. If the check reveals conflicting modifications, the committing transaction rolls back.

 

The use of this concurrency is visible in the functions of the provider implementation, e.g.

 

 
 	 
 

 

 

 

 

 

 

By specifying the AccessCondition property of the BlobRequestOptions object, the code tells Azure Blob Storage not to touch the file if the file has been modified since the last time we looked at it. If the file has been touched, the CloudBlog object from the StorageClient library throws StorageClientException. Since we would like to take such a concurrency error as only a temporary error and would like to sync the blob another time therefore we skip this blob for sync and take up this blob the next time sync occurs.

 

 
 	 
 

 

 

 

 

 



 

 

 

3.     Application Walkthrough

 

3.1        Application Activity Flow

 

 

 

 

 

 

 

 

 

3.2        Activity Description

 

 STARTUP FORM
 

On firing the application user can see the startup form of application. From here the user can navigate to profile options such as create\edit profile or launch sync operation for a profile. The various components of the startup form are shown below:

 

 

 

 

 

 

 

 

 Create New Profile
 

Prior to any sync operation one needs to create a new sync profile with information for mapping a container to a system on the user’s system. Along with this information, some information about sync preferences has to be supplied too. However these preferences may be temporarily changed at runtime. Please note that Auto sync mode is not yet available.

 

 

 

 

 

 Edit Profile
 

You can edit your preferences at any point of time. However, if the profile under edit is currently being synced then the changes would take effect the next time.

 

 

 

 

 

 

 

 

 Sync Operation
 

To start syncing an existing file you need to visit the main window of application, select a profile and click on Sync button. The sync statistics are displayed in a running mode.

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 Exception and Messages
 

If at any point of time an exception rises or a message needs to be displayed, a popup of the following format is displayed.

 

Error\Message Notification Popup
 

Exception Notification Popup
 

 

 



 

 

4.     FUTURE SCOPE

 
