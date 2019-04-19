# Ajuro.Notes


Notes management made easy.

>	A note is actually a file. Befind the scene, **Active Notes** makes it easy to duplicate, rename or delete your files. Along with the files of your notes Active Notes also manages the meta information that accompanies your notes.

--------------------------

## Usage

Organize your notes.

Press Ctrl+N to create a new note.

Press Ctrl+S to save changes.

Press Ctrl+U to upload a note.

Press Ctrl+D to delete a note. 

## Contributors

We are happy to integrate your work if we find it in line with our plans.

Drop a line to have a chat. We are keen to find out your sugestions.

### New in v 1.0.0.3

Upload notes to Cosmos DB with Ctrl+U
Link to this document as help.
Status bar

### New in v 1.0.0.2

Create, Delete, Duplicate notes. When you duplicate a note, the original note remains untouched and your recent changes become a new note that is saved. If you already changes the name of the note, the new name will be used to generate the duplicate. If needed the duplicate neme will be generated based on original name. Notes names has to be unique.



## How it works?

A list of notes is presented. The notes can be presented in the editor. You can edit notes. At the moment you decide to save you can overrite the contect using Ctrl+S or you can Create a new note from the unsaved work and keep the original note unchanged using Ctrl+D

## Nice to have

Planned for next release:

	* SQL notes to run via ODBC.

	* Notes filter by name.

	* Notes tags.

## Known issues

New notes are not inserted in alfabetical order. This issue is acceptable because we are planning to introduce a collection manager for the notes list.

## Usage


Keyboard Shortcuts


| Shortcuts    | Action     |
| :---:         | :---      |
| CTRL+S       | Save       |
| Delete       | Delete     |
| Ctrl+N       | New        |
| Ctrl+D       | Duplicate  |


## One.Server

Open Source

### API

#### ListByAuthor

GET https://ajuro.azurewebsites.net/api/notes/list/1

#### DeleteByRowId

GET https://ajuro.azurewebsites.net/api/notes/delete/ba8c75d9-d300-9fbb-865a-34c292eca4ad

#### UpdateNote

POST https://ajuro.azurewebsites.net/api/notes/update/690d1690-d92c-4c54-9c3b-e604ab5a1125


	{
		"Author": 1,
		"Title": "UpdatedNote",
		"Content": "My Content Updated"
	}

#### InsertNote

POST https://ajuro.azurewebsites.net/api/notes/insert

	{
		"Author": 1,
		"Title": "InsertedNote note key",
		"Content": "My Content Inserted"
	}

