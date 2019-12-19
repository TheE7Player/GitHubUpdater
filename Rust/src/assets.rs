//assets - Handles the release file(s) information
// An attribute to hide warnings for unused code.
#![allow(dead_code)]
#[allow(non_snake_case)]



//Enum which holds how much to divide 1024 by
#[derive(Copy, Clone)]
pub enum DisplaySize { Kilobytes = 2, Megabytes = 3, Gigabytes = 4}

pub struct Asset { itemName: String, itemDownloadUrl: String, itemSize: f64}

//Now we populate the struct (impl = Implement)
impl Asset
{
     //Public function called "getItemSize"
     //With SizeType parameter called "DisplaySize"
     //Which targets itself (&self, in this case, Asset), and returns back (->) as a String
     pub fn get_item_size(&self, SizeType: DisplaySize) -> String
     {
         //(avoids "moved" error)
         let _type = SizeType;

         //Bind it as i16 (Integer) (i8: Short, i16: Int, i32: Long)
         let IterationAmount = _type as i16;

         //Bind initialSize as double (f64) and assign it to structs "itemSize"
         //using "mut" as we want to change the value (let is immutable by default - cannot change after assigned)
         let mut initialSize  = self.itemSize;
         //Now we loop over!
         let mut loopCount = 1;

         loop { initialSize /= 1024 as f64; loopCount += 1; if loopCount == IterationAmount { break; } }

         //{:.2} <- Rounds it to two dp (Decimal Place)
         match _type { DisplaySize::Kilobytes => {format!("{:.2}KB", initialSize)} DisplaySize::Megabytes => {format!("{:.2}MB", initialSize)} DisplaySize::Gigabytes => {format!("{:.2}GB", initialSize)} }
     }
     pub fn get_name(&self) -> &String { &self.itemName }
     pub fn get_url(&self) -> &String { &self.itemDownloadUrl }

     pub fn new(name: String, url: String, size: f64) -> Self { Self { itemName: name, itemDownloadUrl: url, itemSize: size } }
}