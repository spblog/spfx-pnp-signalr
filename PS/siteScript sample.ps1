$site_script = @'
{
    "$schema": "schema.json", 
    "actions": [
		{
				"verb": "triggerFlow",
				"url": "<Logic App trigger url>",
				"name": "Apply PnP Template",
				"parameters": {
					"event":"",
					"product":""
				}
		},
		{
			"verb": "associateExtension",
			"title": "PnP Notifier",
			"location": "ClientSideExtension.ApplicationCustomizer",
			"clientSideComponentId": "260b5bdc-c542-4f9f-868e-bb9d2cd4bc45",
			"scope": "Site"
		}
    ],
    "bindata": {},
    "version": 1
}
'@

Add-SPOSiteScript -Title "Trigger Drone template provisioning" -Content $site_script -Description "Applies drone communication site template via azure web job"

#Set-SPOSiteScript -Identity 3814fab2-a4ed-4d82-b884-5926c5b17c19 -Content $site_script -Version 3