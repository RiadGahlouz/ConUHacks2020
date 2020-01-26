#![feature(proc_macro_hygiene, decl_macro)]

#[macro_use] extern crate rocket;

use std::fs::OpenOptions;
use std::io::prelude::*;

use std::fs::File;
use std::io::{self, BufRead};
use std::path::Path;

use chrono::prelude::*;

use rocket::request::Form;

use serde::{Deserialize, Serialize};
// use serde_json::Result;

#[derive(FromForm, Debug)]
struct UserEmotion {
    id: i8,
    happiness: f64,
    anger: f64,
    contempt: f64,
    disgust: f64,
    fear: f64,
    neutral: f64,
    sadness: f64,
    surprise: f64
}

#[derive(Serialize, Deserialize)]
struct UserEmotionJson {
    id: i8,
    timestamp: i64,
    happiness: f64,
    anger: f64,
    contempt: f64,
    disgust: f64,
    fear: f64,
    neutral: f64,
    sadness: f64,
    surprise: f64
}

#[get("/feels/<id>")]
fn get_feels(id: String) -> String {
    let mut res = String::from("[");

    let filename = format!("custom-db/{}.txt", id);
    if let Ok(lines) = read_lines(filename) {
        // Consumes the iterator, returns an (Optional) String
        for line in lines {
            if let Ok(ip) = line {
                res.push_str(&ip);
                res.push(',');
            }
        }
    }

    res.pop();
    res.push(']');

    res
}

#[post("/feels", data = "<user>")]
fn post_feels(user: Form<UserEmotion>) -> &'static str {
    let to_write_obj = UserEmotionJson {
        id : user.id,
        timestamp: Local::now().timestamp_millis(),
        happiness: user.happiness,
        anger: user.anger,
        contempt: user.contempt,
        disgust: user.disgust,
        fear: user.fear,
        neutral: user.neutral,
        sadness: user.sadness,
        surprise: user.surprise
    };

    let filename = format!("custom-db/{}.txt", user.id);
    let mut file = OpenOptions::new()
                        .append(true)
                        .create(true)
                        .open(filename)
                        .unwrap();

    if let Err(e) = writeln!(file, "{}", serde_json::to_string(&to_write_obj).unwrap()) {
        eprintln!("Couldn't write to file: {}", e);
    }
    // println!("User: {:?}", to_write);
    "Hello, world!"
}

fn read_lines<P>(filename: P) -> io::Result<io::Lines<io::BufReader<File>>>
where P: AsRef<Path>, {
    let file = File::open(filename)?;
    Ok(io::BufReader::new(file).lines())
}

fn main() {
    rocket::ignite().mount("/", routes![get_feels, post_feels]).launch();
}
