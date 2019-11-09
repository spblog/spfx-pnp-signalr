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
    }
    ],
    "bindata": {},
    "version": 1
}
'@

Add-SPOSiteScript -Title "Trigger Drone template provisioning" -Content $site_script -Description "Applies drone communication site template via azure web job"