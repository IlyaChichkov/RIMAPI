# Changelog
## v0.5.6

Fix /api/v1/resources/stored

## v0.5.5

Add endpoint:
[GET]
- /api/v1/materials-atlas
[POST]
- /api/v1/dev/console
- /api/v1/materials-atlas/clear
- /api/v1/stuff/color
- /api/v1/item/image

Minor fixes

## v0.5.4

Add SSE broadcast:
- message_received
- letter_received
- make_recipe_product
- unfinished_destroyed
- date_changed

Updated SSE broadcast:
- colonist_ate

Add endpoint:
- /api/v1/map/rooms
- /api/v1/time-assignments
- /api/v1/colonist/time-assignment
- /api/v1/outfits

SSE service refactoring & fixes
Update debug logging class
Fix loggingLevel config value wasn't save in Scribe_Values 
Add example script for colony food analysis 

## v0.5.3

Add endpoint:
- /api/v1/jobs/make/equip
- /api/v1/pawn/portrait/image
- /api/v1/colonist/work-priority
- /api/v1/colonists/work-priority
- /api/v1/work-list

Optimize resources Dto
Update BaseController CORS header handling

## v0.5.2
Add endpoint:
- /api/v1/resources/stored

Update resources Dto

## v0.5.1
Update skills Dto

## v0.5.0

Add more endpoints
[POST]
- /api/v1/deselect
- /api/v1/deselect
- /api/v1/deselect
- /api/v1/select
- /api/v1/open-tab

[GET]
- /api/v1/map/zones
- /api/v1/map/buildings
- /api/v1/building/info

Update headiffs data in 
- /api/v1/colonists/detailed
- /api/v1/colonist/detailed

Add SSE endpoint:
- 'colonist_ate'

Add SSE client for testing

## v0.4.4

Add camera stream (default: localhost:5001)
Add camera stream endpoints to start, stop, setup, get status

## v0.4.3

Add quests and incidents endpoints
Add pawn opinion about pawn endpoint
Update colonist detailed data

Steam version updated

## v0.4.0

Update research endpoints
Improved dashboard example: https://github.com/IlyaChichkov/rimapi-dashboard

## v0.3.0

Update mod settings

## v0.2.0

Update README, Licence, Github CI/CD
Fix exception handling

## v0.1.0

Add basic endpoints
Add docs
