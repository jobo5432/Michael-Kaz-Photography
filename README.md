# Michael-Kaz-Photography

This repository contains the code for The 'Michael Kaz Photography' website. 

The front end is a C#/MVC web application. The backend is a meld of Google Drive (via Google's RESTful API) and Microsoft Azure.

The Ask
---------------------
My long-time friend Michael is an aspiring photographer. He is a not an aspiring website content manager. When first asked to do this site for him, I spec'ed out the same straightforward website for him but also included a very robust image/gallery manager CMS tool that would allow him to create galleries, image collections, and images - each with captions and titles, and metadata. The CMS tool would not be standalone, but integrated into the frontend so that if he were logged in, the admin features would be visible, overlayed over the content. This would have allowed him to manage his changes and preview them in realtime before committing them.

An old English proverb reads: "Necessity is the mother of invention". If necessity is its mother, then laziness is certainly it's father. In a wireframe walkthrough of my proposed CMS tool, Michael explained that this is too much work for him... (I know, right?) and that he would prefer something less involved. Rather than keep on firing shots until something hit, I decided to meet him at his doorstep and develop a solution that would not require him to do anything beyond what he's already doing... Saving images locally to his hard drive.

The Solution
---------------------
I researched DropBox, Google Drive, Microsoft OneDrive, etc... And settled on Google Drive out of familiarity with the product. I asked him to get himself a Google Drive account, download the client, and create a mapped Google Drive cloud folder somewhere on his computer. This would be his CMS system. All he had to do was create and keep a root folder inside of which has a series of folders, each full of images. Google Drive will sync these images in the cloud, and I could reference them directly for use on the site rather than maintaining a repository in the web file tree.

The Problem
--------------------
I tried to use Google drive like a CDN for his gallery images. While this works, site performance took a serious hit. Images loaded slowly and that was not acceptable on a portfolio site such as this. I had to find a better way to serve these images up. Again, to make this as painless for my friend as possible, I didn't want him to incur any additional steps to publishing his website content. 

My first inclination was to create a cache in the web tree where I could locally store the images found on Google Drive. Since the content is relatively static, I figured it would be easy to create a single admin page where my friend could invoke a rebuild of the local cache. This process would wipe out the cached images/galleries, go and fetch the images/galleries from Google Drive again, and repopulated the cache. This worked perfectly until I deployed the solution to the Azure Websites deployment slot I had created for this project. Azure Websites are not designed to support file IO in this way and as such, pathing on the Azure environment became an issue. 

The Second Solution
-------------------
Rather than try to twist and finagle the web application and Azure instance to make this caching idea work, I decided instead to adhere to the Azure best practices and implement Azure Blob Storage. Here, I can get the images from Google Drive by folder. I create a new Azure Blob Container for each folder (gallery) and a blob for each image in that folder.

When I was creating/deleting local folders in the web tree, things were easy. I could easily forcefully delete a folder and then just recreate it. Azure Blob Containers don't work that way. You have to "lease" these containers, which means that just because you've deleted it doesn't mean the lease expires on the name. Therefore, when you delete a container, you can't use the same name again until the lease expires (which I've found to be much longer than anyone - or any process - should have to wait). Additionally, Azure storage containers have strict naming conventions:

1. Names must be all lowercase
2. Names must include only alphanumerics and dahses ('-') only
3. Names must begin with a letter and end with a letter
4. Where the dash is used, a character must immediately precede and follow the dash itself.

This presents problems as Google Drive does not have those requirements and I was not about to make this a concern for my friend. Here's what solved that problem:

Assuming we are managing the gallery in the Google Drive instance called 'landscape', I have to create a container called 'landscape-date-yyyymmddhhmmss', where the yyymmddhhmmss represents the year month day hour minute second that the container was createde. It solves the name leasing issue, but injects another issue: Maintaining the container names and mappings to the gallery names as specified by my friend in Google Drive. I could have actually kept a mapping (say in an Azure Table or in memory or some accesible serialized form), but instead I just wrote a method to normalize the container name. This lets me pull the containers and extract the original name (by splitting the string on the -date- token) and set up the site's navigation using a normalized container names while using the actual name to fetch the image blobs inside.

Because Azure Storage was meant to be utilized by other Azure offerings (including Websites), hosting these images in Azure makes the site much faster than when it was fed via Google Drive directly. It does add a single additional step for Michael when he wants to add new images. He has invoke the storage management update utility which drops the Azure containers and recreates them based on the current state of the designated Google Drive folder.

What Next?
------------------------
I see no reason why Michael can't just manage his images directly in Azure Storage. Right now, there is no good solution for doing the automatic desktop syncing like Google Drive does. I would like to build this functionality for him. Essentially it will be a service that watches a directory for changes and syncs what it finds with the Azure the storage containers used on the website. 
